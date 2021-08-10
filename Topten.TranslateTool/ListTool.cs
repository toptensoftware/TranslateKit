using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topten.JsonKit;

namespace TranslateTool
{
    class ListTool
    {
        bool _showHelp;
        bool _todo;
        bool _done;
        string _filename;

        /// <summary>
        /// Show command line help
        /// </summary>
        void ShowHelp()
        {
            Program.ShowLogo();
            Console.WriteLine("Usage: TranslateTool list [Options] <filename>");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --todo          List strings that need attention (no translation or machine == true");
            Console.WriteLine("  --done          List strings that have been translated and checked (translated and machine == false)");
            Console.WriteLine("  --help          Show this help screen");
            Console.WriteLine();
            Console.WriteLine("Lists strings from a translation file.");
        }

        /// <summary>
        /// Process a single command line arg
        /// </summary>
        /// <param name="arg">The argument string</param>
        public void ProcessArg(string arg)
        {
            // Args are in format [/--]<switchname>[:<value>];
            if (CommandLineUtils.IsSwitch(arg, out var switchName, out var switchValue))
            {
                switch (switchName)
                {
                    case "help":
                        _showHelp = true;
                        return;

                    case "todo":
                        _todo = true;
                        return;

                    case "done":
                        _done = true;
                        return;

                    default:
                        throw new InvalidOperationException(string.Format("Unknown switch '{0}'", arg));
                }
            }
            else
            {
                if (_filename == null)
                {
                    _filename = arg;
                }
                else
                {
                    throw new InvalidOperationException(string.Format("Unknown command line option - don't know what to do with '{0}'", arg));
                }
            }
        }

        public void ProcessArgs(IEnumerable<string> args)
        {
            // Parse args
            foreach (var a in args)
            {
                ProcessArg(a);
            }
        }

        public int Run(string[] args)
        {
            ProcessArgs(args);
            if (_showHelp || _filename == null)
            {
                ShowHelp();
                return 0;
            }

            // Something to do?
            if (!_done && !_todo)
            {
                Console.Error.WriteLine("Please specify either --todo or --done");
                return 7;
            }

            // Load source file
            var source = Json.ParseFile<Dictionary<string, Phrase>>(_filename);

            // Translate all untranslated phrases
            foreach (var kv in source)
            {
                // List it?
                bool done = !kv.Value.Machine && kv.Value.Translation != null;
                if ((_todo && !done) || (_done && done))
                {
                    Console.WriteLine($"\"{kv.Value}\" => \"{kv.Value.Translation}\"");
                }

                // Contexts
                if (kv.Value.Contexts != null)
                {
                    foreach (var context in kv.Value.Contexts)
                    {
                        done = !context.Value.Machine && context.Value.Translation != null;
                        if ((_todo && !done) || (_done && done))
                        {
                            Console.WriteLine($"\"{kv.Value}\" ({context.Key}) => \"{context.Value.Translation}\"");
                        }
                    }
                }
            }

            return 0;
        }
    }
}
