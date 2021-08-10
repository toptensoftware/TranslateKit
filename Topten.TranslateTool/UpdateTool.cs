using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topten.JsonKit;

namespace TranslateTool
{
    class UpdateTool
    {
        bool _showHelp;
        bool _locations = true;
        bool _trim = true;
        string _sourceFile;
        string _targetFile;

        /// <summary>
        /// Show command line help
        /// </summary>
        void ShowHelp()
        {
            Program.ShowLogo();
            Console.WriteLine("Usage: TranslateTool update [Options] <sourcefile> <targetfile>");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --no-locations  Don't copy location information");
            Console.WriteLine("  --no-trim       Don't delete no longer used strings");
            Console.WriteLine("  --help          Show this help screen");
            Console.WriteLine();
            Console.WriteLine("This command updates the target file with any phrases in the source file");
            Console.WriteLine("that don't exist in the target file.");
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

                    case "no-locations":
                        _locations = false;
                        return;

                    case "no-trim":
                        _trim = false;
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

        void MergeContexts(Dictionary<string, PhraseContext> target, Dictionary<string, PhraseContext> source)
        {
            foreach (var kv in source)
            {
                // Strip out locations
                if (!_locations)
                {
                    kv.Value.Locations = null;
                }

                if (target.TryGetValue(kv.Key, out var targetPhraseContext))
                {
                    // Just update locations leave everything else alone
                    targetPhraseContext.Locations = kv.Value.Locations;
                }
                else
                {
                    target.Add(kv.Key, kv.Value);
                }
            }

            // Remove contexts that no longer exist
            if (_trim)
            {
                foreach (var kv in target.Keys.Where(x => !source.ContainsKey(x)))
                {
                    target.Remove(kv);
                }
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

            Console.WriteLine($"Updating {_targetFile} from {_sourceFile}");

            // Load both files
            var updateFrom = Json.ParseFile<Dictionary<string, Phrase>>(_sourceFile);
            Dictionary<string, Phrase> updateTo;
            if (System.IO.File.Exists(_targetFile))
            {
                updateTo = Json.ParseFile<Dictionary<string, Phrase>>(_targetFile);
            }
            else
            {
                updateTo = new();
            }

            // Remove locations from target file
            foreach (var kv in updateTo)
            {
                kv.Value.Locations = null;
            }

            // Copy targets from source file
            foreach (var kv in updateFrom)
            {
                // Strip out locations?
                if (!_locations)
                    kv.Value.Locations = null;

                Phrase p;
                if (updateTo.TryGetValue(kv.Key, out p))
                {
                    // Copy new locations
                    p.Locations = kv.Value.Locations;

                    // Update contexts
                    if (p.Contexts == null || !p.Contexts.Any())
                    {
                        p.Contexts = kv.Value.Contexts;
                    }
                    else
                    {
                        MergeContexts(p.Contexts, kv.Value.Contexts);
                    }
                }
                else
                {
                    updateTo.Add(kv.Key, kv.Value);
                }
            }

            if (_trim)
            {
                foreach (var k in updateTo.Where(x => !updateFrom.ContainsKey(x.Key)).ToList())
                {
                    updateTo.Remove(k.Key);
                }
            }

            Json.WriteFile(_targetFile, updateTo);

            return 0;
        }
    }
}
