using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TranslateTool
{
    class ExtractTool
    {
        string _fileSpec;
        bool _vsFormat;
        bool _fileHeader;
        bool _raw;
        bool _translationStrings;
        bool _nonTranslationStrings;
        bool _json;
        bool _showHelp;
        bool _jsonLocations;
        string _outputFile;

        /// <summary>
        /// Show command line help
        /// </summary>
        void ShowHelp()
        {
            Program.ShowLogo();
            Console.WriteLine("Usage: TranslateTool extract [Options] <filespec>");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --t           Extract translatable .T() strings");
            Console.WriteLine("  --nt          Extract non-translatable strings");
            Console.WriteLine("  --vs          Output strings in Visual Studio `filename(line): string` format");
            Console.WriteLine("  --json        Output in JSON format");
            Console.WriteLine("  --locations   Include locations in JSON output");
            Console.WriteLine("  --fileheader  Output source file name header");
            Console.WriteLine("  --raw         Don't escape output strings");
            Console.WriteLine("  --out:<file>  Output to file instead of stdout");
            Console.WriteLine("  --help        Show this help screen");
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
                    case "vs":
                        _vsFormat = true;
                        break;

                    case "fileheader":
                        _fileHeader = true;
                        break;

                    case "raw":
                        _raw = true;
                        break;

                    case "t":
                        _translationStrings = true;
                        break;

                    case "nt":
                        _nonTranslationStrings = true;
                        break;

                    case "json":
                        _json = true;
                        break;

                    case "locations":
                        _jsonLocations = true;
                        break;

                    case "out":
                        if (switchValue == null)
                            throw new InvalidOperationException("--out: argument expects a filename");
                        _outputFile = switchValue;
                        break;

                    case "help":
                        _showHelp = true;
                        break;

                    default:
                        throw new InvalidOperationException(string.Format("Unknown switch '{0}'", arg));
                }
            }
            else
            {
                if (_fileSpec == null)
                {
                    _fileSpec = arg;
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
            // Process args
            try
            {
                ProcessArgs(args);
            }
            catch (Exception x)
            {
                Console.Error.WriteLine($"{x.Message}");
                return 7;
            }

            // Just show help
            if (_showHelp || _fileSpec == null)
            {
                ShowHelp();
                return 7;
            }

            // Work out base directory and list of files to scan
            string baseDir = null;
            string[] files = null;
            if (System.IO.Directory.Exists(_fileSpec))
            {
                files = System.IO.Directory.GetFiles(_fileSpec, "*", SearchOption.AllDirectories);
                baseDir = _fileSpec;
            }
            else
            {
                int lastSep = _fileSpec.LastIndexOf(System.IO.Path.DirectorySeparatorChar);
                if (lastSep > 0)
                {
                    baseDir = _fileSpec.Substring(0, lastSep);
                    files = System.IO.Directory.GetFiles(baseDir, _fileSpec.Substring(lastSep + 1), SearchOption.AllDirectories);
                }
                else
                {
                    baseDir = ".";
                    files = System.IO.Directory.GetFiles(".", _fileSpec, SearchOption.AllDirectories);
                }
            }
            baseDir = System.IO.Path.GetFullPath(baseDir);

            // Work out string escaper
            Func<string, string> FormatString = Tokenizer.EscapeString;
            if (_raw)
            {
                FormatString = x => x;
            }

            // Don't include file header if JSON
            if (_json)
            {
                _fileHeader = false;
            }

            // Setup parser
            var parser = new Parser();
            parser.IncludeTranslatableStrings = _translationStrings;
            parser.IncludeNonTranslatableStrings = _nonTranslationStrings;

            // Create output
            TextWriter output;
            if (_outputFile != null)
                output = new StreamWriter(_outputFile, false, Encoding.UTF8);
            else
                output = Console.Out;

            try
            {
                // Process all files
                Dictionary<string, StringInfo> _allStrings = new Dictionary<string, StringInfo>();
                int count = 0;
                foreach (var f in files)
                {
                    try
                    {
                        // Open tokenizer
                        bool headerWritten = false;

                        // Work out file location string
                        string location = f;
                        if (location.StartsWith(baseDir))
                        {
                            location = location.Substring(baseDir.Length);
                        }

                        foreach (var s in parser.ParseFile(f))
                        {

                            // Write header if first string in file
                            if (!headerWritten && _fileHeader)
                            {
                                output.WriteLine(f);
                                headerWritten = true;
                            }

                            // Add string to the context
                            if (!_allStrings.TryGetValue(s.str, out var si))
                            {
                                si = new StringInfo(s.str);
                                _allStrings.Add(s.str, si);
                            }

                            if (s.context == null)
                            {
                                // Unqualified string
                                si.locations.Add(location);
                            }
                            else
                            {
                                // String qualified by key
                                List<string> locations;
                                if (!si.contexts.TryGetValue(s.str, out locations))
                                {
                                    locations = new List<string>();
                                    si.contexts.Add(s.context, locations);
                                }
                                locations.Add(location);
                            }

                            // Output string
                            if (_vsFormat)
                            {
                                // VS Format
                                if (s.context == null)
                                    output.WriteLine($"{f}({s.LineNumber + 1 }): {FormatString(s.str)}");
                                else
                                    output.WriteLine($"{f}({s.LineNumber + 1}): {FormatString(s.str)} ({FormatString(s.context)})");
                            }
                            else if (_json)
                            {
                                // We'll write JSON format later
                            }
                            else
                            {
                                // Just output the string
                                if (s.context == null)
                                    output.WriteLine($"{FormatString(s.str)}");
                                else
                                    output.WriteLine($"{FormatString(s.str)} ({FormatString(s.context)})");
                            }

                            count++;
                        }

                        // Separator
                        if (_fileHeader && headerWritten)
                        {
                            output.WriteLine();
                        }
                    }
                    catch (Exception x)
                    {
                        Console.Error.WriteLine("Exception: {0}\nwhile processing file {1}", x.Message, f);
                        return 7;
                    }
                }

                // Output JSON format
                if (_json)
                {
                    output.WriteLine("{");
                    foreach (var kv in _allStrings)
                    {
                        output.WriteLine("{0}:", Tokenizer.EscapeString(kv.Key));
                        output.WriteLine("{");
                        if (_jsonLocations)
                        {
                            output.WriteLine("\t\"locations\": [");
                            foreach (var location in kv.Value.locations.Distinct())
                            {
                                output.WriteLine("\t\t{0},", Tokenizer.EscapeString(location));
                            }
                            output.WriteLine("\t],");
                        }
                        if (kv.Value.contexts.Count > 0)
                        {
                            output.WriteLine("\t\"contexts\":");
                            output.WriteLine("\t{");
                            foreach (var k in kv.Value.contexts)
                            {
                                output.WriteLine("\t\t{0}:", Tokenizer.EscapeString(k.Key));
                                output.WriteLine("\t\t{");
                                if (_jsonLocations)
                                {
                                    output.WriteLine("\t\t\t\"locations\": [");
                                    foreach (var location in k.Value.Distinct())
                                    {
                                        output.WriteLine("\t\t\t\t{0},", Tokenizer.EscapeString(location));
                                    }
                                    output.WriteLine("\t\t\t],");
                                }
                                output.WriteLine("\t\t},");
                            }
                            output.WriteLine("\t},");
                        }
                        output.WriteLine("},");
                    }
                    output.WriteLine("}");
                }
                else
                {
                    output.WriteLine("\nFinished: {0} strings found, {1} are unique\n", count, _allStrings.Count);
                }
            }
            finally
            {
                // Close output
                if (output != Console.Out)
                    output.Dispose();
            }
            return 0;
        }
    }
}
