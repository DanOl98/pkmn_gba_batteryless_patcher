using BatterylessPatcherLib;

internal class Program
{
    private static void printHelp()
    {
        Console.WriteLine();
        Console.WriteLine($"Usage:");
        Console.WriteLine();
        Console.WriteLine($"{"-i or -input inputfilename".PadRight(30)} | specify the input ROM");
        Console.WriteLine();
        Console.WriteLine($"{"-o or -output outputfilename".PadRight(30)} | specify where the output ROM will be saved (optional, if not specified, the output file will be saved in the same directory as the input file with the .batteryless.gba extension)");
        Console.WriteLine();
        Console.WriteLine($"{"-s or -save savefilename".PadRight(30)} | specify the input save data file to inject (.sav) (optional)");
        Console.WriteLine();
        Console.WriteLine($"{"-fr or -forcerepack".PadRight(30)} | force a full hard repack of the ROM (UNSAFE!! could break some ROMs)");
        Console.WriteLine();
        Console.WriteLine($"{"-h or -help".PadRight(30)} | print this help");
        Console.WriteLine();

    }
    private static void Main(string[] args)
    {
        String filename = "";
        String outputname = "";
        String savetoinject = "";
        bool forcerepack = false;
        if (args.Length == 0)
        {
            printHelp();
            return;
        }

        for (var i = 0; i < args.Length; i++)
        {    
            switch (args[i])
            {
                case "-i":
                case "-input":
                    {
                        if (i + 1 < args.Length)
                        {
                            filename = args[i + 1];
                            i++;
                        }
                        else
                        {
                            Console.WriteLine("Missing value for specified parameter -i or -input");
                            return;
                        }
                        break;
                    }
                case "-s":
                case "-save":
                    {
                        if (i + 1 < args.Length)
                        {
                            savetoinject = args[i + 1];
                            i++;
                        }
                        else
                        {
                            Console.WriteLine("Missing value for specified parameter -s or -save");
                            return;
                        }
                        break;
                    }
                case "-o":
                case "-output":
                    {
                        if (i + 1 < args.Length)
                        {
                            outputname = args[i + 1];
                            i++;
                        }
                        else
                        {
                            Console.WriteLine("Missing value for specified parameter -o or -output");
                            return;
                        }
                        break;
                    }
                case "-fr":
                case "-forcerepack":
                    {
                        forcerepack = true;
                        break;
                    }
                case "-h":
                case "-help":
                    {
                        printHelp();
                        return;
                    }
                default:
                    {
                        Console.WriteLine($"Unrecognized arg {args[i]}");
                        return;
                    }
            }
        }
        if (filename == "")
        {
            Console.WriteLine("ROM file not specified");
            return;
        };
        byte[] rom = [];
        byte[] savedata = [];
        if (!File.Exists(filename))
        {
            Console.WriteLine("Cannot find requested ROM in filesystem");
            return;
        }
        else
        {
            rom = File.ReadAllBytes(filename);
        }
        if (!File.Exists(savetoinject) && savetoinject != "")
        {
            Console.WriteLine("Cannot find requested save data in filesystem");
            return;
        }
        else if (File.Exists(savetoinject))
        {
            savedata = File.ReadAllBytes(savetoinject);
        }

        if (outputname == "")
        {
            outputname = Path.Combine(
               Path.GetDirectoryName(filename)!,
               Path.GetFileNameWithoutExtension(filename) + ".batteryless.gba"
           );
        };

        Console.WriteLine($"[INFO] Starting to patch {filename}");

        try
        {
            var outputData = PatcherFRLGITA.ApplyBatterylessPatch(rom, savedata, forcerepack);
            File.WriteAllBytes(outputname, outputData);
            Console.WriteLine($"[INFO] Saved in {outputname}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERR] Message: {ex.Message}");
            Console.WriteLine($"[ERR] Stacktrace: {ex.StackTrace}");

        }
        finally
        {
        }
    }


}