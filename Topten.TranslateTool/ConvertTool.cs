using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topten.JsonKit;

namespace TranslateTool
{
    public class PhraseInfoV1
    {
        /// <summary>
        /// The phrase context
        /// </summary>
        [Json("contexts")]
        public List<string> Context;

        [Json("translation")]
        public string Translation;

        [Json("machine")]
        public bool Machine;
    }

    class ConvertTool
    {
        bool _showHelp;
        string _sourceFile;
        string _targetFile;

        /// <summary>
        /// Show command line help
        /// </summary>
        void ShowHelp()
        {
            Program.ShowLogo();
            Console.WriteLine("Usage: translatetool convert [Options] <sourcefile> <targetfile>");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --help          Show this help screen");
            Console.WriteLine();
            Console.WriteLine("Converts an old format strings JSON file to the new format");
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

                    default:
                        throw new InvalidOperationException(string.Format("Unknown switch '{0}'", arg));
                }
            }
            else
            {
                if (_sourceFile == null)
                {
                    _sourceFile = arg;
                }
                else if (_targetFile == null)
                {
                    _targetFile = arg;
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
            if (_showHelp || _targetFile == null || _sourceFile == null)
            {
                ShowHelp();
                return 0;
            }

            Console.WriteLine($"Converting {_sourceFile} to {_targetFile}");

            // Load both files
            var phraseMapV1 = Json.ParseFile<Dictionary<string, PhraseInfoV1>>(_sourceFile);

            var phraseList = phraseMapV1.Select(kv =>
            {
                return new PhraseInfo()
                {
                    Phrase = kv.Key,
                    Locations = kv.Value.Context,
                    Translation = kv.Value.Translation,
                    Machine = kv.Value.Machine,
                };
            }).ToList();

            Json.WriteFile(_targetFile, phraseList);

            return 0;
        }
    }
}
