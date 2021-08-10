using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topten.JsonKit;

namespace TranslateTool
{
    class MachineTranslateTool
    {
        bool _showHelp;
        bool _redo;
        bool _all;
        string _apiKey;
        string _filename;
        string _language;

        /// <summary>
        /// Show command line help
        /// </summary>
        void ShowHelp()
        {
            Program.ShowLogo();
            Console.WriteLine("Usage: TranslateTool translate [Options] <filename>");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --help          Show this help screen");
            Console.WriteLine("  --redo          Also re-translate existing machine translations");
            Console.WriteLine("  --all           Re-translate all strings");
            Console.WriteLine("  --apikey:key    Google Translate API key");
            Console.WriteLine("  --language:iso  The target language (if not specified, expects filename in format strings-xx.json)");
            Console.WriteLine();
            Console.WriteLine("This command machine translates the specified file.");
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

                    case "language":
                        _language = switchValue;
                        break;

                    case "redo":
                        _redo = true;
                        break;

                    case "all":
                        _all = true;
                        break;

                    case "apikey":
                        _apiKey = switchValue;
                        break;

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


            // Work out language
            if (_language == null)
            {
                var name = System.IO.Path.GetFileNameWithoutExtension(_filename);
                var sep = name.IndexOf('-');
                if (sep > 0)
                    _language = name.Substring(sep + 1);
            }

            if (string.IsNullOrEmpty(_language))
            {
                throw new InvalidOperationException("No language specified and couldn't determine from file name");
            }

            if (_apiKey == null)
            {
                throw new InvalidOperationException("Google Translate API Key not set");
            }
            GoogleTranslate.ApiKey = _apiKey;

            Console.WriteLine($"Translating {_filename} to {_language}");

            // Load source file
            var source = Json.ParseFile<Dictionary<string, Phrase>>(_filename);

            // Translate all untranslated phrases
            foreach (var kv in source)
            {
                // Main term
                if (kv.Value.Translation == null || _all || (_redo && kv.Value.Machine))
                {
                    kv.Value.Translation = GoogleTranslate.Translate(kv.Key, _language);
                    kv.Value.Machine = true;
                }

                // Contexts
                if (kv.Value.Contexts != null)
                {
                    foreach (var context in kv.Value.Contexts)
                    {
                        if (context.Value.Translation == null || _all || (_redo && context.Value.Machine))
                        {
                            if (kv.Value.Machine)
                            {
                                context.Value.Translation = kv.Value.Translation;
                                context.Value.Machine = true;
                            }
                            else
                            {
                                context.Value.Translation = GoogleTranslate.Translate(kv.Key, _language); ;
                                context.Value.Machine = true;
                            }
                        }
                    }
                }
            }

            Json.WriteFile(_filename, source);
            return 0;
        }
    }
}
