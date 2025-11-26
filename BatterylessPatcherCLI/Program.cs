using BatterylessPatcherLib;

internal class Program
{
    private static void Main(string[] args)
    {
        String filename = "";
        String outputname = "";
        String savetoinject = "";
        bool forcerepack = false;
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
                default:
                    {
                        Console.WriteLine($"Unrecognized arg {args[i]}");
                        break;
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