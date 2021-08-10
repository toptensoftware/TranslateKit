using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace TranslateTool
{
    class Program
    {
        public static void ShowLogo()
        {
            Console.WriteLine("TranslateTool v{0}", Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine("Copyright © 2014-2021 Topten Software. All Rights Reserved");
            Console.WriteLine();
        }

        static void ShowHelp()
        {
            Console.WriteLine("Usage: TranslateTool [extract|translate|<options>]");
            Console.WriteLine();
            Console.WriteLine("    extract      extracts strings from C# files");
            Console.WriteLine("    update       updates a translated file with newly extracted strings");
            Console.WriteLine("    translate    machine translates any untranslated strings");
            Console.WriteLine("    list         lists strings from a translation file");
            Console.WriteLine("  --help         show this help");
            Console.WriteLine("  --version      show version information");
        }

        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                ShowLogo();
                ShowHelp();
                return 0;
            }

            var arg = args[0];
            if (CommandLineUtils.IsSwitch(arg, out var switchName, out var switchValue))
            {
                if (switchName == "version")
                {
                    ShowLogo();
                    return 0;
                }
                if (switchName == "help")
                {
                    ShowLogo();
                    ShowHelp();
                    return 0;
                }
            }
            else
            {
                if (arg == "extract")
                {
                    return new ExtractTool().Run(args.Skip(1).ToArray());
                }
                if (arg == "update")
                {
                    return new UpdateTool().Run(args.Skip(1).ToArray());
                }
                if (arg == "translate")
                {
                    return new MachineTranslateTool().Run(args.Skip(1).ToArray());
                }
                if (arg == "list")
                {
                    return new ListTool().Run(args.Skip(1).ToArray());
                }
            }

            Console.Error.WriteLine($"Unknown command line option: {arg}");
            return 7;
        }


    }
}
