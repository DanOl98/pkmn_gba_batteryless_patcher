using static BatterylessPatcherLib.Utils;

namespace BatterylessPatcherLib
{

    public class Repack
    {
        const uint PTR_BASE = 0x08000000;
        const byte FILL = 0xFF;

        class Options
        {
            public int Align = 0x04;
            public bool AlsoThumb = true;
            
        }

        public static byte[] repack(byte[] data, List<ForbiddenArea> forbiddenAreas, bool soft = false, string TruncateMode = "auto")
        {
            byte[] originalData = new byte[data.Length];
            // copia
            Buffer.BlockCopy(data, 0, originalData, 0, data.Length);
            var opt = new Options();

            int n = data.Length;
            Console.WriteLine($"[WARN] WARNING!!! Repacking on heavily modified ROMs could break something");
            Console.WriteLine($"[INFO] {(soft? "Soft" : "Hard" )} repacking");
            Console.WriteLine($"[INFO] ROM size = {n} (0x{n:X})");
            List<LzBlock> allLzBlocks = ScanAllLzBlocks(data, 0, 0, opt);
            uint scanStart = 0x01000000;
            //trovo primo blocco intorno a 0x01000000 per provare a relocare solo i blocchi oltre i 16M
            if (soft)
            {
                //tutti i blocchi LZ validi
                //they are already ordered by offset so no need to resort
                for (int i = 0; i < allLzBlocks.Count; i++)
                {
                    var blk = allLzBlocks[i];
                    int end = blk.getEnd();
                    if ((end > 0x01000000 && blk.Offset < 0x01000000) || blk.Offset >= 0x01000000)
                    {
                        //my starting point
                        scanStart = (uint)blk.Offset;
                        break;
                    }
                }
            }
            //tutti i blocchi LZ validi
            List<LzBlock> toRelocateBlocks = new List<LzBlock>();
            if (soft)
            {
                
                for (int i = 0; i < allLzBlocks.Count; i++)
                {
                    var blk = allLzBlocks[i];
                    if (blk.Offset >= scanStart)
                    {
                        toRelocateBlocks.Add(blk);
                    }else if (blk.Offset % 4 != 0)
                    {
                        //also fix blocks in wrong positions because why not, some ROMs are broken because of this
                        toRelocateBlocks.Add(blk);
                    }
                }
            }
            else
            {
                for (int i = 0; i < allLzBlocks.Count; i++)
                {
                    var blk = allLzBlocks[i];
                    toRelocateBlocks.Add(blk);
                }
            }
            //Console.WriteLine($"[INFO] LZ blocks found: {allLzBlocks.Count}");
            Console.WriteLine($"[INFO] LZ blocks to relocate: {toRelocateBlocks.Count}");
            //clear all
            for (int i = 0; i < toRelocateBlocks.Count; i++)
            {
                Console.Write($"\r[CLEAR] clearing blocks - {(i + 1).ToString().PadLeft(5, '0')}/{toRelocateBlocks.Count.ToString().PadLeft(5, '0')}");
                var blk = toRelocateBlocks[i];
                int src = blk.Offset;
                int size = blk.Size;
                // azzera sorgente (PRIMA SE NO SOVRASCRIVO ROBA COPIATA SOPRA!!

                for (int x = 0; x < size; x++)
                    data[src + x] = FILL;

                //if empty/padding, clear area after
                int clearAfter = 0;
                int checkStart = (blk.getEnd() + 1);
                if (i < toRelocateBlocks.Count - 1)
                {
                    LzBlock next = toRelocateBlocks[i + 1];

                    int difference = next.Offset - checkStart;

                    if (Utils.FreeAtConsideringZeroValid(checkStart, difference, data) >= difference)
                    {
                        clearAfter = difference;
                    }
                }
                for (int x = 0; x < clearAfter; x++)
                    data[checkStart + x] = FILL;


            }
            if(toRelocateBlocks.Count>0) Console.Write("\r\n");

            //ordino per size in modo da non andarmi sopra
            toRelocateBlocks.Sort((a, b) =>
             {
                 return a.Size.CompareTo(b.Size);
             });
            //allLz = allLz.OrderBy(o => o.Size).ToList();
            var firstLz = allLzBlocks.OrderBy(o => o.Offset).ToList().First();
            int lastSize = 0;
            int movedRef = 0;
            int movedOrph = 0, movedFromForbidden = 0;
            int lastMoved = firstLz.Offset;
            for (int i = 0; i < toRelocateBlocks.Count; i++)
            {
                var blk = toRelocateBlocks[i];
                uint addr = PTR_BASE + (uint)blk.Offset;
                var refs = ScanPointerPositionsFiltered(data, addr, opt.AlsoThumb, allLzBlocks);
                int src = blk.Offset;
                int size = blk.Size;
                int dst = Utils.FindFirstFreeForSprite(data, size, lastMoved, opt.Align, forbiddenAreas, allLzBlocks, blk);
                if (soft)
                {
                    while(dst != -1)
                    {
                        LzBlock? blockInside = Utils.isInsideBlock(dst, dst + size -1, allLzBlocks);
                        if (blockInside == null)
                        {
                            break;
                        }
                        else
                        {
                            lastMoved = blockInside.getEnd() + 1;
                            dst = Utils.FindFirstFreeForSprite(data, size, lastMoved, opt.Align, forbiddenAreas, toRelocateBlocks, blk);
                        }
                    }
                }
                uint oldAddr = PTR_BASE + (uint)src;
                ForbiddenArea? inArea = isInForbiddenArea(src, src + size, forbiddenAreas);
                if (inArea != null)
                {
                    movedFromForbidden++;
                }
                if (dst == -1)
                {
                    Console.WriteLine($"[ERR] invalid size at offset 0x{blk.Offset:x}, size {blk.Size}, expanded {blk.ExpandedSize} looked like {blk.type}");
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
                blk.Offset = dst;
                lastSize = size;
                foreach (var (off, isThumb) in refs)
                {
                    uint v = newAddr;
                    if (opt.AlsoThumb && isThumb) v |= 1;
                    Utils.WriteU32(data, off, v);
                }

                if (refs.Count > 0)
                {
                    movedRef++;
                }
                else
                {
                    movedOrph++;
                }
                Console.Write($"\r[MOVE] moving blocks - {(i + 1).ToString().PadLeft(5, '0')}/{toRelocateBlocks.Count.ToString().PadLeft(5, '0')}");
                //Console.Write($"\r[MOVE] moving blocks - {(i + 1).ToString().PadLeft(5, '0')}/{toRelocateBlocks.Count.ToString().PadLeft(5, '0')} (0x{(src.ToString("X").PadLeft(6, '0'))})");
                //modifico per quando controllo puntatori per evitare di andare a scrivere un finto puntatore in una zona dentro a un blocco
            }
            if (toRelocateBlocks.Count > 0) Console.Write("\r\n");

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
            if (allLzBlocks.Count != blocks2.Count)
            {
                String err = $"[ERR] Blocks count changed: old {allLzBlocks.Count} and new {blocks2.Count}";
                Console.WriteLine(err);
                throw new InvalidOperationException(err);
            }
            bool skipTruncate = TruncateMode == "no";
            //skip if rom is already 16m and should truncate to 16m
            skipTruncate = skipTruncate || (TruncateMode == "16m" && data.Length == 0x01000000);
            //skip if rom is already 32m and should truncate to 32m
            skipTruncate = skipTruncate || (TruncateMode == "32m" && data.Length == 0x02000000);
            // truncate opzionale
            if (!skipTruncate)
            {

                //Console.WriteLine($"[INFO] LZ blocks found: {blocks2.Count}");
                int highestRefTrunc = data.Length;
                int highestOrphanTrunc = data.Length;
                if (TruncateMode != "16m" && TruncateMode != "32m" && TruncateMode != "auto")
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
                int truncatedSize = -1;
                if (TruncateMode == "refs")
                {
                    truncatedSize = highestRefTrunc;
                }
                else if (TruncateMode == "keep_orphans")
                {
                    truncatedSize = Math.Max(highestRefTrunc, highestOrphanTrunc);
                }
                else if (TruncateMode == "16m")
                {
                    truncatedSize = 0x01000000;
                }
                else if (TruncateMode == "32m")
                {
                    truncatedSize = 0x02000000;
                }
                else if (TruncateMode == "auto")
                {
                    //sometimes in the end there is junk (some 00)
                    if (Utils.IsEmptyAfterOffsetConsideringZeroValid(0x01000000, data))
                    {
                        truncatedSize = 0x01000000;
                    }
                    else if (Utils.IsEmptyAfterOffsetConsideringZeroValid(0x02000000, data))
                    {
                        truncatedSize = 0x02000000;
                    }
                }
                if (truncatedSize != -1)
                {
                    int bytesToRemove = data.Length - truncatedSize;
                    //sometimes in the end there is junk (some 00)

                    int freebytes = Utils.FreeAtConsideringZeroValid(truncatedSize, bytesToRemove, data);
                    if (freebytes >= bytesToRemove)
                    {
                        byte[] data2 = new byte[truncatedSize];
                        Buffer.BlockCopy(data, 0, data2, 0, truncatedSize);
                        data = data2;
                        Console.WriteLine($"[INFO] truncated to {truncatedSize}");
                    }
                    else
                    {
                        Console.WriteLine($"[ERR] cannot truncate at requested mode - data not empty ({freebytes}/{bytesToRemove})");
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
            //in some roms, some lz blocks do not start at %4, so cannot search just %4 position
            for (int i = (int)scanStart; i < n; i++)
            {
                if (data[i] == 0x10)
                {
                    bool isInPointer = false;
                    if (i % 4 != 0)
                    {
                        uint val = Utils.ReadU32(data, (i - (i % 4)));
                        if ((val & 0xFF000000) == 0x08000000 || (val & 0xFF000000) == 0x09000000)
                        {
                            isInPointer = true;
                        }
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
                        else
                        {
                            //debug
                            //Console.WriteLine($"[INFO] invalid block at {i:X}, size {size}");                 
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

                    bool use = true;
                    int x = i;
                    while (x + 1 < count)
                    {
                        LzBlock next = possibleBlocks[x + 1];
                        if (!Utils.isSafeInBlock(next.Offset, next.getEnd(), blk))
                        {
                            //NEED SOME WAY TO DISTINGUISH REAL POINTERS FROM SHIT THAT LOOKS LIKE POINTERS
                            //ALSO NEEDED TO PATCH THEM!

                            /*uint addr = PTR_BASE + (uint)blk.Offset;
                            bool refs = isPointerPresentFiltered(data, addr, opt.AlsoThumb, list);
                            uint addrNext = PTR_BASE + (uint)next.Offset;
                            bool refsNext = isPointerPresentFiltered(data, addrNext, opt.AlsoThumb, list);
                            if(refs && !refsNext)
                            {
                                x++;
                            }
                            else if (refsNext && !refs)
                            {
                                use = false;
                                break;
                            }
                            else
                            {
                                x++;
                            }*/
                            //Console.WriteLine($"[INFO] skipping next for priority 0x{blk.Offset:X}-{blk.Size}-{blk.ExpandedSize}-{blk.type} | 0x{next.Offset:X}-{next.Size}-{next.ExpandedSize}-{next.type}");
                            //i++;
                            x++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (use)
                    {
                        list.Add(new LzBlock(blk.Offset, blk.Size, blk.ExpandedSize, blk.type));
                        i = x;
                    }

                    //}

                }
                Console.Write($"\r[INFO] current progress {i + 1}/{count} - found {list.Count}");
                //Console.Write($"\r[INFO] current progress {i + 1}/{count} - 0x{blk.Offset:X} - found {list.Count}");
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

        static bool isPointerPresentFiltered(byte[] data, uint targetAddr, bool alsoThumb, List<LzBlock> allLz)
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
                    if (Utils.isInsideBlock(i, i + 4, allLz) == null)
                    {
                        return true;
                    }
                    else
                    {
                        //skip fake pointers (bytes that loooked like a pointer to that but were data inside a lz block)
                        //Console.WriteLine($"[WARN] pointer was inside block, skipped");
                    }
                }
                else if (alsoThumb &&
                         data[i] == wordThumb[0]
                      && data[i + 1] == wordThumb[1]
                      && data[i + 2] == wordThumb[2]
                      && data[i + 3] == wordThumb[3])
                {
                    if (Utils.isInsideBlock(i, i + 4, allLz) == null)
                    {
                        return true;
                    }
                    else
                    {
                        //skip fake pointers (bytes that loooked like a pointer to that but were data inside a lz block)
                        //Console.WriteLine($"[WARN] pointer was inside block, skipped");
                    }
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
        static List<(int Off, bool IsThumb)> ScanPointerPositionsFiltered(byte[] data, uint targetAddr, bool alsoThumb, List<LzBlock> allLz)
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
                    if (Utils.isInsideBlock(i, i + 4, allLz) == null)
                    {
                        res.Add((i, false));
                    }
                    else
                    {
                        //skip fake pointers (bytes that loooked like a pointer to that but were data inside a lz block)
                        //Console.WriteLine($"[WARN] pointer was inside block, skipped");
                    }     
                }
                else if (alsoThumb &&
                         data[i] == wordThumb[0]
                      && data[i + 1] == wordThumb[1]
                      && data[i + 2] == wordThumb[2]
                      && data[i + 3] == wordThumb[3])
                {
                    if (Utils.isInsideBlock(i, i + 4, allLz) == null)
                    {
                        res.Add((i, true));
                    }
                    else
                    {
                        //skip fake pointers (bytes that loooked like a pointer to that but were data inside a lz block)
                        //Console.WriteLine($"[WARN] pointer was inside block, skipped");
                    }
                }
            }
            
            return res;
        }

    }
}
