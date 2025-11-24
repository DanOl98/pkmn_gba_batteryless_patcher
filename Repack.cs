using static BatterylessPatcher.Utils;

namespace BatterylessPatcher
{

    public class Repack
    {
        const uint PTR_BASE = 0x08000000;
        const byte FILL = 0xFF;

        class Options
        {
            public int Align = 0x04;
            public bool AlsoThumb = true;
            public string TruncateMode = "auto"; // "no", "refs", "keep_orphans"
        }


        public static byte[] repack(byte[] data, List<ForbiddenArea> forbiddenAreas)
        {
            byte[] originalData = new byte[data.Length];
            // copia
            Buffer.BlockCopy(data, 0, originalData, 0, data.Length);
            var opt = new Options();

            int n = data.Length;
            Console.WriteLine($"[INFO] ROM size = {n} (0x{n:X})");

            //tutti i blocchi LZ validi
            List<LzBlock> allLz = ScanAllLzBlocks(data, 0, 0, opt);
            Console.WriteLine($"[INFO] LZ blocks found: {allLz.Count}");
            //clear all
            for (int i = 0; i < allLz.Count; i++)
            {
                Console.Write($"\r[CLEAR] clearing blocks - {(i + 1).ToString().PadLeft(5, '0')}/{allLz.Count.ToString().PadLeft(5, '0')}");
                var blk = allLz[i];
                int src = blk.Offset;
                int size = blk.Size;
                uint addr = PTR_BASE + (uint)blk.Offset;
                // azzera sorgente (PRIMA SE NO SOVRASCRIVO ROBA COPIATA SOPRA!!

                for (int x = 0; x < size; x++)
                    data[src + x] = FILL;

            }
            Console.Write("\r\n");

            //ordino per size in modo da non andarmi sopra
            allLz.Sort((a, b) =>
             {   
                 return a.Size.CompareTo(b.Size);
             });
            //allLz = allLz.OrderBy(o => o.Size).ToList();
            var firstLz = allLz.OrderBy(o => o.Offset).ToList().First();
            int lastSize = 0;
            int movedRef = 0;
            int movedOrph = 0, movedFromForbidden = 0;
            int lastMoved = firstLz.Offset;
            for (int i = 0; i < allLz.Count; i++)
            {
                var blk = allLz[i];
                uint addr = PTR_BASE + (uint)blk.Offset;
                var refs = ScanPointerPositions(data, addr, opt.AlsoThumb);
                int src = blk.Offset;
                int size = blk.Size;
                int dst = Utils.FindFirstFreeForSprite(data, size, lastMoved, opt.Align, forbiddenAreas, allLz, blk);
                uint oldAddr = PTR_BASE + (uint)src;
                ForbiddenArea? inArea = isInForbiddenArea(src, src + size, forbiddenAreas);
                if (inArea != null)
                {
                    movedFromForbidden++;
                }
                if (dst == -1)
                {
                    byte[] decompressed = Utils.LzDecompress(originalData, (int)blk.Offset);
                    int looksLike = Utils.IsLikelyValidLz(decompressed);
                    
                    Console.WriteLine($"[ERR] invalid size at offset 0x{blk.Offset:x}, size {blk.Size}, expanded {blk.ExpandedSize}, size {decompressed.Length}, looked like {looksLike}");
                    continue;
                }

                // copia (dall'originale perché potrebbe essere stato sovrascritto)
                Buffer.BlockCopy(originalData, src, data, dst, size);
                if (dst + size > lastMoved)
                {
                    lastMoved = dst + size;
                }
                // aggiorna puntatori
                uint newAddr = PTR_BASE + (uint)dst;
                //var refs2 = ScanPointerPositions(originalData, addr, opt.AlsoThumb);
                //modifico nel nuovo
                foreach (var (off, isThumb) in refs)
                {
                    uint v = newAddr;
                    if (Utils.isInsideBlock(off, off + 4, allLz)==null)
                    {
                        if (opt.AlsoThumb && isThumb) v |= 1;
                        Utils.WriteU32(data, off, v);
                    }
                    else
                    {
                        //skip fake pointers (bytes that loooked like a pointer to that but were data inside a lz block)
                        //Console.WriteLine($"[WARN] pointer was inside block, skipped");
                    }
                }

                if (refs.Count > 0)
                {
                    movedRef++;
                }
                else
                {
                    movedOrph++;
                }
                Console.Write($"\r[MOVE] moving blocks - {(i + 1).ToString().PadLeft(5, '0')}/{allLz.Count.ToString().PadLeft(5, '0')} (0x{(src.ToString("X").PadLeft(6, '0'))})");
                //modifico per quando controllo puntatori per evitare di andare a scrivere un finto puntatore in una zona dentro a un blocco
                blk.Offset = dst;
                lastSize = size;
            }
            Console.Write("\r\n");

            Console.WriteLine($"[SUMMARY] moved with refs = {movedRef} | moved orphans = {movedOrph}");

            foreach (ForbiddenArea area in forbiddenAreas)
            {
                int freebytes = Utils.FreeAtConsideringZeroValid(area.Start, area.Length, data);
                if (freebytes >= area.Length)
                {
                    Console.WriteLine($"[OK] forbidden area clear (0x{area.Start:X} to 0x{(area.Start + area.Length):X})");
                }
                else
                {
                    Console.WriteLine($"[ERR] forbidden area NOT clear (0x{area.Start:X} to 0x{(area.Start + area.Length):X}). free bytes {freebytes}/{area.Length}");
                }
            }


            Console.WriteLine($"[INFO] rescanning LZ blocks");
            var blocks2 = ScanAllLzBlocks(data, 0, 0, opt);
            if (allLz.Count != blocks2.Count)
            {
                String err = $"[ERR] Blocks count changed: old {allLz.Count} and new {blocks2.Count}";
                Console.WriteLine(err);
                throw new InvalidOperationException(err);
            }
            bool skipTruncate = opt.TruncateMode == "no";
            //skip if rom is already 16m and should truncate to 16m
            skipTruncate = skipTruncate || (opt.TruncateMode == "16m" && data.Length == 0x01000000);
            //skip if rom is already 32m and should truncate to 32m
            skipTruncate = skipTruncate || (opt.TruncateMode == "32m" && data.Length == 0x02000000);
            // truncate opzionale
            if (!skipTruncate)
            {
               
                Console.WriteLine($"[INFO] LZ blocks found: {blocks2.Count}");
                int highestRefTrunc = data.Length;
                int highestOrphanTrunc = data.Length;
                if (opt.TruncateMode != "16m" && opt.TruncateMode != "32m" && opt.TruncateMode!= "auto")
                {
                    for (int i = 0; i < blocks2.Count; i++)
                    {
                        var blk = blocks2[i];
                        uint addr = PTR_BASE + (uint)blk.Offset;
                        var refs = isPointerPresent(data, addr, opt.AlsoThumb);
                        if (refs)
                        {
                            highestRefTrunc = Utils.AlignUp(blk.Offset + blk.Size, 0x1000);

                        }
                        else
                        {
                            highestOrphanTrunc = Utils.AlignUp(blk.Offset + blk.Size, 0x1000);
                        }
                        Console.Write($"\r[SCAN] scanning to truncate - {(i + 1).ToString().PadLeft(5, '0')}/{blocks2.Count.ToString().PadLeft(5, '0')} (0x{(blk.Offset.ToString("X").PadLeft(6, '0'))})");
                    }
                }
                int truncSize = -1;
                if (opt.TruncateMode == "refs")
                {
                    truncSize = highestRefTrunc;
                }
                else if (opt.TruncateMode == "keep_orphans")
                {
                    truncSize = Math.Max(highestRefTrunc, highestOrphanTrunc);
                }
                else if (opt.TruncateMode == "16m")
                {
                    truncSize = 0x01000000;
                }
                else if (opt.TruncateMode == "32m")
                {
                    truncSize = 0x02000000;
                }
                else if (opt.TruncateMode == "auto")
                {
                    //sometimes in the end there is junk (some 00)
                    if(Utils.IsEmptyAfterOffsetConsideringZeroValid(0x01000000, data))
                    {
                        truncSize = 0x01000000;
                    }else if (Utils.IsEmptyAfterOffsetConsideringZeroValid(0x02000000, data))
                    {
                        truncSize = 0x02000000;
                    }
                }
                int truncLen = data.Length - truncSize;
                //sometimes in the end there is junk (some 00)
                if (truncLen != data.Length)
                {
                    int freebytes = Utils.FreeAtConsideringZeroValid(truncSize, truncLen, data);
                    if (freebytes >= truncLen)
                    {
                        byte[] data2 = new byte[truncSize];
                        Buffer.BlockCopy(data, 0, data2, 0, truncSize);
                        data = data2;
                        Console.WriteLine($"[INFO] truncated to {truncSize}");
                    }
                    else
                    {
                        Console.WriteLine($"[ERR] cannot truncate at requested mode - data not empty ({freebytes}/{truncLen})");
                    }
                }
                else
                {
                    Console.WriteLine($"[INFO] no need to truncate");
                }
            }
            Console.WriteLine($"[INFO] repacked");
            return data;
        }



        static List<LzBlock> ScanAllLzBlocks(byte[] data, uint scanStart, int minSize, Options opt)
        {
            var list = new List<LzBlock>();
            var possibleBlocks = new List<LzBlock>();
            int n = data.Length;
            Console.WriteLine($"[INFO] scanning possible LZ blocks");
            for (int i = (int)scanStart; i < n; i++)
            {
                if (data[i] == 0x10)
                {
                    bool isInPointer = false;
                    uint val = Utils.ReadU32(data, (i - (i % 4)));
                    if ((val & 0xFF000000) == 0x08000000)
                    {
                        isInPointer = true; 
                    }
                    if (!isInPointer)
                    {
                        int size = Utils.LzSize(data, (int)i);
                        if (size > 0)
                        {

                            byte[] decompressed = Utils.LzDecompress(data, i);
                            int type = Utils.IsLikelyValidLz(decompressed);
                            if (type >= 0)
                            {
                                possibleBlocks.Add(new LzBlock(i, size, decompressed.Length, type));
                            }
                        }
                    }
                }
            }

            int count = possibleBlocks.Count;
            Console.WriteLine($"[INFO] found {count} possible LZ blocks");
            Console.WriteLine($"[INFO] scanning possible LZ blocks for valid LZ data");

            for (int i = 0; i < count; i++)
            {
                LzBlock blk = possibleBlocks[i];

                bool inBlock = false;
                for (int j = 0; j < list.Count; j++)
                {
                    //se fa parte di un blocco già identificato
                    if (!Utils.isSafeInBlock(blk.Offset, blk.getEnd(), list[j]))
                    {
                        inBlock = true;
                        break;
                    }
                }
                if (!inBlock)
                {


                    list.Add(new LzBlock(blk.Offset, blk.Size, blk.ExpandedSize, blk.type));
                    while (i + 1 < count)
                    {
                        LzBlock next = possibleBlocks[i + 1];
                        if (!Utils.isSafeInBlock(next.Offset, next.getEnd(), blk))
                        {
                            //Console.WriteLine($"[INFO] skipping next for precedence 0x{blk.Offset:X}-{blk.Size}-{blk.ExpandedSize}-{blk.type} | 0x{next.Offset:X}-{next.Size}-{next.ExpandedSize}-{next.type}");
                            i++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    //}

                }
                Console.Write($"\r[INFO] current progress {i + 1}/{count} - 0x{blk.Offset:X} - found {list.Count}");
            }
            Console.Write("\r\n");
            return list;
        }


       
        static bool isPointerPresent(byte[] data, uint targetAddr, bool alsoThumb)
        {
            int n = data.Length;
            var res = new List<(int, bool)>();
            byte[] word = BitConverter.GetBytes(targetAddr);
            byte[] wordThumb = BitConverter.GetBytes(targetAddr | 1);
            for (int i = 0; i <= n - 4; i += 4)
            {
                if (data[i] == word[0]
                    && data[i + 1] == word[1]
                    && data[i + 2] == word[2]
                    && data[i + 3] == word[3])
                {
                    return true;
                }
                else if (alsoThumb &&
                         data[i] == wordThumb[0]
                      && data[i + 1] == wordThumb[1]
                      && data[i + 2] == wordThumb[2]
                      && data[i + 3] == wordThumb[3])
                {
                    return true;
                }
            }
            return false;
        }

        static List<(int Off, bool IsThumb)> ScanPointerPositions(byte[] data, uint targetAddr, bool alsoThumb)
        {
            int n = data.Length;
            var res = new List<(int, bool)>();
            byte[] word = BitConverter.GetBytes(targetAddr);
            byte[] wordThumb = BitConverter.GetBytes(targetAddr | 1);
            for (int i = 0; i <= n - 4; i += 4)
            {
                if (data[i] == word[0]
                    && data[i + 1] == word[1]
                    && data[i + 2] == word[2]
                    && data[i + 3] == word[3])
                {
                    res.Add((i, false));
                }
                else if (alsoThumb &&
                         data[i] == wordThumb[0]
                      && data[i + 1] == wordThumb[1]
                      && data[i + 2] == wordThumb[2]
                      && data[i + 3] == wordThumb[3])
                {
                    res.Add((i, true));
                }
            }
            return res;
        }

    }
}
