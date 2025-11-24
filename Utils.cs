namespace BatterylessPatcher
{
    public class Utils
    {
        public const uint ROM_BASE = 0x08000000;

        public static void CopyBlobAndFixInternals(byte[] rom, byte[] blob, int oldOffset, int newOffset, Dictionary<uint, byte[]> toReplaceCalls, Dictionary<uint, byte[]> toReplaceCallsFirst)
        {
            // copy blob
            Buffer.BlockCopy(blob, 0, rom, newOffset, blob.Length);

            // calculate delta to fix interna pointers
            uint oldBase = ROM_BASE + (uint)oldOffset;
            uint newBase = ROM_BASE + (uint)newOffset;
            uint delta = newBase - oldBase;
            // scan for pointers
            for (int i = 0; i < blob.Length; i += 4)
            {
                uint pos = (uint)(newOffset + i);

                uint val = (uint)(
                    rom[pos] |
                    (rom[pos + 1] << 8) |
                    (rom[pos + 2] << 16) |
                    (rom[pos + 3] << 24)
                );
                // Fix internal pointers
                if (val >= oldBase && val < oldBase + (uint)blob.Length)
                {
                    uint newVal = val + delta;
                    PatchJump(rom, pos, newVal);
                }
                //fix known external pointers (idk what some are, they don't look like anything with sense on the original rom. the games work without repointing those, maybe leftover in the blob for other games? idk)
                else if((val & 0xFF000000) == 0x08000000)
                {
                    if (toReplaceCalls.ContainsKey(val))
                    {
                        byte[] toFind = toReplaceCalls[val];
                        int found = Utils.FindPatternForPatch(rom, toFind, true);
                        uint newVal = (uint)found + ROM_BASE;
                        PatchJump(rom, pos, newVal);
                    }
                    else if (toReplaceCallsFirst.ContainsKey(val))
                    {
                        byte[] toFind = toReplaceCallsFirst[val];
                        int found = Utils.FindPatternForPatch(rom, toFind, false);
                        uint newVal = (uint)found + ROM_BASE;
                        PatchJump(rom, pos, newVal);
                    }
                    
                }
            }
        }

        public static void PatchSaveBase(byte[] rom, int blobOffset, int blobSize, byte oldSaveBase, byte newSaveBase)
        {
            // Cerca pattern: [oldsavebase] XX A0 E3 (MOV R0,#0x78)
            // e sostituisce [oldsavebase] -> [newsavebase]
            try
            {
                for (int i = blobOffset; i < blobOffset + blobSize - 3; i++)
                {
                    if (rom[i] == oldSaveBase && rom[i + 2] == 0xA0 && rom[i + 3] == 0xE3)
                    {
                        rom[i] = newSaveBase;
                    }
                }
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                throw e;
            }
        }



        public static void PatchJump(byte[] rom, uint pos, uint newTargetAddr)
        {

            rom[pos] = (byte)(newTargetAddr & 0xFF);
            rom[pos + 1] = (byte)((newTargetAddr >> 8) & 0xFF);
            rom[pos + 2] = (byte)((newTargetAddr >> 16) & 0xFF);
            rom[pos + 3] = (byte)((newTargetAddr >> 24) & 0xFF);
        }
        public static void PatchROMStartOffsetToBlob(byte[] rom, uint targetOffset)
        {
            if (rom == null || rom.Length < 4)
                throw new ArgumentException("ROM troppo corta.");

            // PC dell'istruzione 0 all'esecuzione (ARM: PC = addr + 8)
            uint pc = Utils.ROM_BASE + 0x00000008;
            uint dest = Utils.ROM_BASE + (uint)targetOffset;   // indirizzo assoluto di destinazione

            // offset relativo in word (imm24 è signed, shiftato di 2)
            int rel = checked((int)(dest - pc)) >> 2;

            // istruzione B AL: cond=1110 (E), opcode branch=1010 (A) → 0xEA000000
            uint instr = 0xEA000000u | ((uint)rel & 0x00FFFFFFu);

            // scrivi in little endian nei primi 4 byte
            rom[0] = (byte)(instr & 0xFF);
            rom[1] = (byte)((instr >> 8) & 0xFF);
            rom[2] = (byte)((instr >> 16) & 0xFF);
            rom[3] = (byte)((instr >> 24) & 0xFF);
        }

        public static int FreeAt(int off, int size, byte[] rom)
        {
            if (off < 0 || off + size > rom.Length) return 0;
            int fr = 0;
            for (int i = off; i < off + size; i++) if (rom[i] != 0xFF) return fr; else fr++;
            return fr;
        }
        public static int FreeAtConsideringZeroValid(int off, int size, byte[] rom)
        {
            if (off < 0 || off + size > rom.Length) return 0;
            int fr = 0;
            for (int i = off; i < off + size; i++) if (rom[i] != 0xFF && rom[i] != 0x00) return fr; else fr++;
            return fr;
        }
        public static int FindFirstFreeForBlob(byte[] rom, int size, int preferred, int minStart, String type = "blob", int positionMultipleOf = 0x1000, List<ForbiddenArea> ? forbiddenAreas = null)
        {
            if (size <= 0) throw new ArgumentException($"Dimensione {type} nulla.");
            
            if (preferred > 0 && FreeAtConsideringZeroValid(preferred, size, rom) >= size) return preferred;

            // Allinea il minStart al successivo multiplo di positionMultipleOf
            int offStart = Math.Max(0, minStart);
            if ((offStart % positionMultipleOf) != 0)
                offStart += positionMultipleOf - (offStart % positionMultipleOf);
            // Cerca solo su offset multipli di 0x10000
            for (int off = offStart; off + size <= rom.Length; off += positionMultipleOf)
            {
                if (forbiddenAreas != null)
                {
                    ForbiddenArea? inArea = isInForbiddenArea(off, off + size, forbiddenAreas);
                    if (inArea != null)
                    {
                        off = Utils.AlignUp(inArea.End, positionMultipleOf) - ((inArea.End % positionMultipleOf) == 0 ? 0 : positionMultipleOf);
                        continue;
                    }
                }
                if (FreeAtConsideringZeroValid(off, size, rom) >= size)
                {
                    return off;
                }
                else
                {
                    off = Utils.AlignUp(off += size, positionMultipleOf) - positionMultipleOf;
                }
            }

            throw new InvalidOperationException($"Nessuna regione FF abbastanza grande per {type}.");
        }
        public class ForbiddenArea
        {
            public int Start;
            public int Length;
            public int End;
            public ForbiddenArea(int Start, int Length)
            {
                this.Start = Start;
                this.Length = Length;
                this.End = this.Start + this.Length;
            }
        }
        public static bool isSafeInBlock(int start, int end, LzBlock blk)
        {
            //solo se inizia prima e finisce prima o inizia dopo e finisce dopo
            return (start < blk.Offset && end < blk.Offset || start > blk.getEnd() && end > blk.getEnd());
        }
        static bool isSafeInForbiddenArea(int start, int end, ForbiddenArea area)
        {
            //solo se inizia prima e finisce prima o inizia dopo e finisce dopo
            return (start < area.Start && end < area.Start || start > area.End && end > area.End);
        }
        public static ForbiddenArea? isInForbiddenArea(int start, int end, List<ForbiddenArea> forbiddenAreas)
        {
            foreach (ForbiddenArea area in forbiddenAreas)
            {
                if (!isSafeInForbiddenArea(start, end, area))
                {
                    return area;
                }
            }
            return null;
        }


        public static LzBlock? isInsideBlock(int start, int end, List<LzBlock> blocks)
        {
            foreach (LzBlock block in blocks)
            {
    
                    if (!isSafeInBlock(start, end, block))
                    {
                        return block;
                    }
                
            }
            return null;
        }
        public class LzBlock
        {
            public int Offset;
            public int Size;
            public int ExpandedSize;
            public int type;
            public LzBlock(int Offset, int Size, int ExpandedSize, int type)
            {
                this.Offset = Offset;
                this.Size = Size;
                this.ExpandedSize = ExpandedSize;
                this.type = type;
            }
            public int getEnd()
            {
                return Offset + Size - 1;
            }
        }

   
        public static int IsLikelyValidLz(byte[] decompressed)
        {


            if (LooksLike4bppTiles(decompressed)) return 1;
            if (LooksLike8bppTiles(decompressed)) return 2;
            if (LooksLikeText(decompressed)) return 3;
            if (LooksLikePalette(decompressed)) return 4;

            return -1;
        }

        static bool LooksLike8bppTiles(byte[] dec)
        {
            if (dec == null) return false;

            int len = dec.Length;

            // minimo: 1 tile 8bpp (64 byte)
            if (len < 64) return false;

            // deve essere multiplo di 64
            if (len % 64 != 0) return false;

            int nTiles = len / 64;

            int totalZeroLike = 0;          // 0x00 e 0xFF
            int tilesAllSameByte = 0;       // tile completamente uniformi
            int tilesWithRepeatedRows = 0;  // tile con almeno una riga identica ad un'altra
            int tilesHighlyRandom = 0;      // tile che sembrano rumore totale

            byte[] tile = new byte[64];

            for (int t = 0; t < nTiles; t++)
            {
                Buffer.BlockCopy(dec, t * 64, tile, 0, 64);

                // 1) conta 0x00 / 0xFF
                int zeroLikeThis = 0;
                for (int i = 0; i < 64; i++)
                {
                    if (tile[i] == 0x00 || tile[i] == 0xFF)
                        zeroLikeThis++;
                }
                totalZeroLike += zeroLikeThis;

                // 2) tile completamente uniforme?
                bool allSame = true;
                for (int i = 1; i < 64; i++)
                {
                    if (tile[i] != tile[0])
                    {
                        allSame = false;
                        break;
                    }
                }
                if (allSame)
                {
                    tilesAllSameByte++;
                    continue; // niente altro da analizzare
                }

                // 3) righe ripetute
                // ogni riga 8bpp = 8 byte (8 pixel)
                bool hasRepeatedRow = false;
                int diffSumBetweenRows = 0;
                int rowComparisons = 0;

                for (int row = 0; row < 8; row++)
                {
                    int offRow = row * 8;

                    if (row > 0)
                    {
                        int offPrev = (row - 1) * 8;
                        bool equal = true;
                        int diffBytes = 0;

                        for (int b = 0; b < 8; b++)
                        {
                            if (tile[offRow + b] != tile[offPrev + b])
                            {
                                equal = false;
                                diffBytes++;
                            }
                        }

                        if (equal)
                            hasRepeatedRow = true;

                        diffSumBetweenRows += diffBytes;
                        rowComparisons++;
                    }
                }

                if (hasRepeatedRow)
                    tilesWithRepeatedRows++;

                // 4) random-ness: se pochissimi 0/FF e righe tutte diversissime → rumore
                double avgDiff = rowComparisons > 0
                    ? (double)diffSumBetweenRows / rowComparisons
                    : 0.0;

                if (zeroLikeThis <= 4 && avgDiff >= 7.0) // quasi tutti i byte diversi
                {
                    tilesHighlyRandom++;
                }
            }

            double fracZeros = (double)totalZeroLike / len;
            double fracUniform = (double)tilesAllSameByte / nTiles;
            double fracRepeatedRows = (double)tilesWithRepeatedRows / nTiles;
            double fracRandom = (double)tilesHighlyRandom / nTiles;

            // Heuristica globale (simile ai 4bpp ma tarata su 8bpp):

            // pochissimi 0x00/0xFF → difficile che sia tiles (secondo me in Pokémon è raro)
            if (fracZeros < 0.01) return false;      // meno dell'1% → sospetto

            // se praticamente tutto è uniforme ma pieno di zeri → blocco di sfondo, può essere valido
            if (fracUniform > 0.95 && fracZeros > 0.5)
                return true;

            // vogliamo almeno un po' di righe ripetute
            if (fracRepeatedRows < 0.05) return false;

            // troppi tile “super random” → sembra dati / rumore
            if (fracRandom > 0.50) return false;

            return true;
        }


        static bool LooksLike4bppTiles(byte[] dec)
        {
            if (dec == null) return false;

            int len = dec.Length;

            // minimo: 1 tile (32 byte)
            if (len < 32) return false;

            // deve essere multiplo di 32 (tile 8x8 4bpp)
            if (len % 32 != 0) return false;

            int nTiles = len / 32;

            int totalZeroLike = 0;          // 0x00 e 0xFF (sfondo/tile vuoti)
            int tilesAllSameByte = 0;       // tile completamente uniformi (tutti i byte uguali)
            int tilesWithRepeatedRows = 0;  // tile che hanno almeno una riga uguale a un'altra
            int tilesHighlyRandom = 0;      // tile che sembrano completamente "rumore"

            // per analisi per-tile
            byte[] tile = new byte[32];

            for (int t = 0; t < nTiles; t++)
            {
                Buffer.BlockCopy(dec, t * 32, tile, 0, 32);

                // 1) conta 0x00 / 0xFF
                int zeroLikeThis = 0;
                for (int i = 0; i < 32; i++)
                {
                    if (tile[i] == 0x00 || tile[i] == 0xFF)
                        zeroLikeThis++;
                }
                totalZeroLike += zeroLikeThis;

                // 2) tile completamente uniforme?
                bool allSame = true;
                for (int i = 1; i < 32; i++)
                {
                    if (tile[i] != tile[0])
                    {
                        allSame = false;
                        break;
                    }
                }
                if (allSame)
                {
                    tilesAllSameByte++;
                    continue; // niente altro da analizzare, è chiaramente "piatto"
                }

                // 3) righe ripetute (pattern grafico: linee simili)
                // ogni riga = 4 byte (8 pixel 4bpp)
                bool hasRepeatedRow = false;
                int diffSumBetweenRows = 0;  // somma differenze tra righe adiacenti
                int rowComparisons = 0;

                for (int row = 0; row < 8; row++)
                {
                    int offRow = row * 4;
                    // confronta con riga precedente
                    if (row > 0)
                    {
                        int offPrev = (row - 1) * 4;
                        bool equal = true;
                        int diffBytes = 0;
                        for (int b = 0; b < 4; b++)
                        {
                            if (tile[offRow + b] != tile[offPrev + b])
                            {
                                equal = false;
                                diffBytes++;
                            }
                        }
                        if (equal)
                            hasRepeatedRow = true;

                        diffSumBetweenRows += diffBytes;
                        rowComparisons++;
                    }
                }

                if (hasRepeatedRow)
                    tilesWithRepeatedRows++;

                // 4) "randomness" grezza: se ogni riga è completamente diversa da quella prima,
                //    e non c'è nessuna 0x00/0xFF, probabilmente è rumore
                double avgDiff = rowComparisons > 0
                    ? (double)diffSumBetweenRows / rowComparisons
                    : 0.0;

                // se zeroLikeThis è molto basso e le righe sono tutte molto diverse,
                // lo segniamo come tile "rumoroso"
                if (zeroLikeThis <= 2 && avgDiff >= 3.5) // quasi tutti i byte diversi
                {
                    tilesHighlyRandom++;
                }
            }

            // statistiche globali
            double fracZeros = (double)totalZeroLike / len;               // percentuale 0x00/0xFF
            double fracUniform = (double)tilesAllSameByte / nTiles;       // tile completamente piatti
            double fracRepeatedRows = (double)tilesWithRepeatedRows / nTiles;
            double fracRandom = (double)tilesHighlyRandom / nTiles;

            // Heuristica:
            // - vogliamo almeno un po' di 0x00/0xFF (sfondo), ma non tutto così
            // - vogliamo che almeno qualche tile abbia righe ripetute
            // - non vogliamo che la maggior parte dei tile sembri rumore puro

            // troppo pochi 0x00/0xFF → sospetto (grafica Pokémon di solito ha molto sfondo trasparente/0)
            if (fracZeros < 0.02) return false;  // <2% bytes 0/FF → poco probabile sia tiles

            // tutto piattume e basta può essere o tutto sfondo, o blocco non usato,
            // ma di solito i blocchi grafici contengono almeno un po' di forme
            if (fracUniform > 0.95 && fracZeros > 0.5)
            {
                // caso borderline: potrebbe essere un blocco di soli tile vuoti,
                // lo accettiamo ma con dubbi. Qui decidi tu se vuoi TRUE o FALSE.
                return true;
            }

            // vogliamo almeno un po' di righe ripetute (forme orizzontali, superfici)
            if (fracRepeatedRows < 0.05) return false;  // meno del 5% dei tile con righe uguali → sembra random / testo

            // se più di metà dei tile sono "super random", è quasi certamente rumore/altro
            if (fracRandom > 0.50) return false;

            return true;
        }


        static bool LooksLikeText(byte[] dec)
        {
            if (dec == null || dec.Length < 4)
                return false;

            int printable = 0;
            int customAllowed = 0; // 0xA0–0xF0 (accenti, simboli Pokémon)
            int terminators = 0;   // 0x00 o 0xFF

            for (int i = 0; i < dec.Length; i++)
            {
                byte b = dec[i];

                // ASCII stampabile classico
                if (b >= 0x20 && b <= 0x7E)
                {
                    printable++;
                }
                // occasionali caratteri custom Pokémon (lettere, accenti, ♦, PKMN symbol, ecc.)
                else if (b >= 0xA0 && b <= 0xF0)
                {
                    customAllowed++;
                }
                // terminatori di stringa
                else if (b == 0x00 || b == 0xFF)
                {
                    terminators++;
                }
                // caratteri di controllo permessi: CR (0x0D), LF (0x0A), TAB (0x09)
                else if (b == 0x0D || b == 0x0A || b == 0x09)
                {
                    printable++; // li conto come accettabili
                }
                else
                {
                    // byte “sporco”, binario puro: non sembra testo
                    return false;
                }
            }

            int len = dec.Length;
            double pPrintable = (double)printable / len;
            double pCustom = (double)customAllowed / len;
            double pTerminators = (double)terminators / len;

            // condizione minima di validità "testuale"
            if (pPrintable < 0.60) return false;    // almeno 60% caratteri stampabili
            if (pCustom > 0.20) return false;       // non più del 20% caratteri custom
            if (pTerminators > 0.10) return false;  // non più del 10% terminatori

            return true;
        }

        static bool LooksLikePalette(byte[] dec)
        {
            if (dec.Length % 2 != 0) return false;
            int numColors = dec.Length / 2;
            if (numColors != 16 && numColors != 256) return false; // o allarghi

            int ok = 0;
            for (int i = 0; i < numColors; i++)
            {
                ushort c = (ushort)(dec[2 * i] | (dec[2 * i + 1] << 8));
                if ((c & 0x8000) != 0) return false; // bit "extra" settato di solito è 0
                int r = c & 0x1F;
                int g = (c >> 5) & 0x1F;
                int b = (c >> 10) & 0x1F;
                if (r <= 31 && g <= 31 && b <= 31) ok++;
            }
            // richiedi che "abbastanza" colori abbiano RGB dentro range
            return ok > numColors * 3 / 4;
        }

        public static int FindFirstFreeForSprite(byte[] rom, int size, int minStart,int alignTo, List<ForbiddenArea> forbiddenAreas, List<LzBlock> allLz, LzBlock currentBlock)
        {
           
            minStart = Utils.AlignUp(minStart, alignTo); 
            for (int off = minStart; off + size <= rom.Length; off += alignTo)
            {
                ForbiddenArea? inArea = isInForbiddenArea(off, off + size, forbiddenAreas);
                if (inArea != null)
                {
                    off = Utils.AlignUp(inArea.End, alignTo) - ((inArea.End%alignTo)==0? 0 : alignTo);
                    continue;
                }

                if (FreeAt(off, size, rom)>= size)
                {

                    return off;
                }
                else
                {
                    off = Utils.AlignUp(off += size, alignTo) - alignTo;
                }
            }
            return -1;
        }
        public static int AlignUp(int value, int align)
        {
            if (align <= 1) return value;
            return ((value + align - 1) / align) * align;
        }

        public static int FindPatternForPatch(byte[] rom, byte[] pattern, bool mustBeUnique)
        {
            int first = FindPattern(rom, pattern, 0);
            if (first < 0) return -1;
            if (mustBeUnique)
            {
                int next = FindPattern(rom, pattern, first + 1);
                if (next >= 0) return -1;
            }
            return first;
        }

 

        public static List<int> FindAll(byte[] rom, byte[] pattern)
        {
            List<int> list = new List<int>();  
            int n = rom.Length - pattern.Length;
            for (int i = 0; i <= n; i++)
            {
                if(rom[i] == pattern[0])
                {
                    int j = 0;
                    for (j = 0; j < pattern.Length; j++)
                    {
                        if (rom[i + j] != pattern[j]) break;
                    }
                    if (j == pattern.Length)
                    {
                        list.Add(i);
                    }
                }
            }
            return list;
        }

        public static uint ReadU32(byte[] data, int offset)
        {
            return BitConverter.ToUInt32(data, offset);
        }

        public static void WriteU32(byte[] data, int offset, uint value)
        {
            byte[] tmp = BitConverter.GetBytes(value);
            Buffer.BlockCopy(tmp, 0, data, offset, 4);
        }

        public static int LzSize(byte[] data, int o)
        {
            int n = data.Length;
            if (o < 0 || o + 4 > n || data[o] != 0x10) return -1;
            int outLen = data[o + 1] | (data[o + 2] << 8) | (data[o + 3] << 16);
            if (outLen <= 0 || outLen > 8 * 1024 * 1024) return -1; // sanity

            int si = o + 4;
            int produced = 0;

            while (produced < outLen)
            {
                if (si >= n) return -1; // finita la ROM
                byte flags = data[si++];

                for (int bit = 0; bit < 8; bit++)
                {
                    if (produced >= outLen) break;

                    if ((flags & (0x80 >> bit)) == 0)
                    {
                        // literal
                        if (si >= n) return -1;
                        si++;
                        produced++;
                    }
                    else
                    {
                        // copy
                        if (si + 1 >= n) return -1;
                        byte hi = data[si++];
                        byte lo = data[si++];

                        int L = (hi >> 4) + 3;
                        int D = ((hi & 0x0F) << 8) | lo;

                        if (D == 0 || D > produced) return -1;
                        produced += L;
                    }
                }
            }

            // se arrivi qui, il blocco è formalmente valido
            return si - o;  // dimensione del blocco compresso
        }



        public static byte[] LzDecompress(byte[] data, int o)
        {
            int n = data.Length;
            if (o < 0 || o + 4 > n || data[o] != 0x10) return null;
            int outLen = data[o + 1] | (data[o + 2] << 8) | (data[o + 3] << 16);
            if (outLen <= 0 || outLen > 8 * 1024 * 1024) return null; // sanity
            int si = o + 4;
            int produced = 0;
            var outBuf = new List<byte>(outLen);

            while (produced < outLen)
            {
                if (si >= n) return null;
                byte flags = data[si++];
                for (int bit = 0; bit < 8; bit++)
                {
                    if (produced >= outLen) break;
                    if ((flags & (0x80 >> bit)) == 0)
                    {
                        if (si >= n) return null;
                        outBuf.Add(data[si++]);
                        produced++;
                    }
                    else
                    {
                        if (si + 1 >= n) return null;
                        byte hi = data[si++];
                        byte lo = data[si++];
                        int L = (hi >> 4) + 3;
                        int D = ((hi & 0x0F) << 8) | lo;
                        if (D == 0 || D > outBuf.Count) return null;
                        for (int k = 0; k < L; k++)
                        {
                            byte b = outBuf[outBuf.Count - D];
                            outBuf.Add(b);
                            produced++;
                            if (produced >= outLen) break;
                        }
                    }
                }
            }
            return outBuf.ToArray();
        }


        public static int FindPattern(byte[] hay, byte[] needle, int start)
        {
            int n = hay.Length - needle.Length;
            for (int i = start; i <= n; i++)
            {
                int j = 0;
                for (; j < needle.Length; j++)
                    if (hay[i + j] != needle[j]) break;
                if (j == needle.Length) return i;
            }
            return -1;
        }

        public static void ApplyPatch1(byte[] rom, int at, uint target)
        {
            byte[] stub;
            stub = new byte[] { 0x00, 0x48, 0x00, 0x47 }; // LDR r0,[pc]; BX r0
            Buffer.BlockCopy(stub, 0, rom, at, 4);
            byte[] word = BitConverter.GetBytes(target);
            Buffer.BlockCopy(word, 0, rom, at + 4, 4);
        }
        public static void ApplyPatch2(byte[] rom, int at, uint target)
        {
            byte[] stub;
            stub = new byte[] { 0x00, 0x4C, 0x20, 0x47 }; // LDR r4,[pc]; BX r4
            Buffer.BlockCopy(stub, 0, rom, at, 4);
            byte[] word = BitConverter.GetBytes(target);
            Buffer.BlockCopy(word, 0, rom, at + 4, 4);
        }
        public static void ApplyPrePatch3(byte[] rom, int at, int length)
        {
            byte[] stub = new byte[] { 0xC0, 0x46 };
            for (int i = 0; i < length / 2; i++)
            {
                Buffer.BlockCopy(stub, 0, rom, at + i * 2, 2);

            }
        }
        public static void ApplyPatch3(byte[] rom, int at, uint target)
        {
            byte[] stub;
            stub = new byte[] { 0x00, 0x49, 0x08, 0x47 }; //
            Buffer.BlockCopy(stub, 0, rom, at, 4);
            byte[] word = BitConverter.GetBytes(target);
            Buffer.BlockCopy(word, 0, rom, at + 4, 4);
        }


        public static int FindCheckAndPatch(byte[] rom, byte[] preContext, byte[] checkOrig, byte[] checkPatch)
        {
            int pos = FindPattern(rom, preContext, 0);
            if (pos < 0)
            {
                Console.WriteLine("[PATCH] RTC Check not found");
                return -1;
            }
            int chkAt = pos + preContext.Length;
            if (!Matches(rom, chkAt, checkOrig))
            {
                int start = pos + 1;
                bool found = false;
                while (true)
                {
                    pos = FindPattern(rom, preContext, start);
                    if (pos < 0) break;
                    chkAt = pos + preContext.Length;
                    if (Matches(rom, chkAt, checkOrig)) { found = true; break; }
                    start = pos + 1;
                }
                if (!found)
                {
                    Console.WriteLine("[PATCH] RTC Check not found");
                    return -1;
                }
                Console.WriteLine("[PATCH] RTC Check patch applied");
            }
            Buffer.BlockCopy(checkPatch, 0, rom, chkAt, checkPatch.Length);
            return chkAt;
        }

        public static bool Matches(byte[] rom, int at, byte[] pattern)
        {
            if (at < 0 || at + pattern.Length > rom.Length) return false;
            for (int i = 0; i < pattern.Length; i++)
                if (rom[at + i] != pattern[i]) return false;
            return true;
        }


    }

}
