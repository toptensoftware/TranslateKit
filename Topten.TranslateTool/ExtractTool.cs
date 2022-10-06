using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Topten.JsonKit;

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
        List<Regex> _regex = new List<Regex>();

        /// <summary>
        /// Show command line help
        /// </summary>
        void ShowHelp()
        {
            Program.ShowLogo();
            Console.WriteLine("Usage: translatetool extract [Options] <filespec>");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --t           Extract translatable .T() strings");
            Console.WriteLine("  --nt          Extract non-translatable strings");
            Console.WriteLine("  --regex:<rx>  Extract strings matching regex (use \\C to match C# style string)");
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

                    case "regex":
                        _regex.Add(new Regex(switchValue.Replace("\\C", @"(?:(?:""(?:\\""|.)*?"")|@""(?:""""|[^""])*"")")));
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
                Dictionary<(string, string), PhraseInfo> _allPhrases = new Dictionary<(string,string), PhraseInfo>();
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

                        foreach (var s in parser.ParseFile(f, _regex))
                        {

                            // Write header if first string in file
                            if (!headerWritten && _fileHeader)
                            {
                                output.WriteLine(f);
                                headerWritten = true;
                            }

                            // Add string to the context
                            if (!_allPhrases.TryGetValue((s.Phrase, s.Context ?? ""), out var p))
                            {
                                p = new PhraseInfo();
                                p.Phrase = s.Phrase;
                                p.Context = s.Context;
                                p.Comment = s.Comment;
                                p.Locations = new List<string>();
                                if (_jsonLocations && !p.Locations.Contains(location))
                                    p.Locations.Add(location);

                                _allPhrases.Add((p.Phrase, p.Context ?? ""), p);
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(s.Comment))
                                {
                                    if (string.IsNullOrEmpty(p.Comment))
                                    {
                                        p.Comment = s.Comment;
                                    }
                                    else
                                    {
                                        if (p.Comment != s.Comment)
                                        {
                                            Console.Error.WriteLine($"Warning: different comments for '{p.Phrase}' ({p.Context}), ignoring '{p.Comment}'");
                                        }
                                    }
                                }
                                if (_jsonLocations && !p.Locations.Contains(location))
                                    p.Locations.Add(location);
                            }

                            // Output string
                            if (_json)
                            {
                                // We'll write JSON format later
                            }
                            else
                            {
                                if (_vsFormat)
                                {
                                    // VS Format
                                    output.Write($"{f}({s.LineNumber + 1 }): ");
                                }

                                output.Write(FormatString(s.Phrase));

                                if (s.Context != null)
                                {
                                    output.Write($" ({FormatString(s.Context)})");
                                }

                                if (s.Comment != null)
                                {
                                    output.Write($" // {s.Comment}");
                                }

                                output.WriteLine();
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
                    Json.Write(output, _allPhrases.Values);
                }
                else
                {
                    output.WriteLine("\nFinished: {0} strings found, {1} are unique\n", count, _allPhrases.Count);
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
