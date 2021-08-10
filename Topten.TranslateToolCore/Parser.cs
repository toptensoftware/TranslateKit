using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslateTool
{
    /// <summary>
    /// Helper class to parse a C# file and locate all instances
    /// of "string".T() and "string".T("context")
    /// </summary>
    public class Parser
    {
        /// <summary>
        /// Constructs a new parser
        /// </summary>
        public Parser()
        {
        }

        /// <summary>
        /// Specifies whether to include translatable .T() strings
        /// </summary>
        public bool IncludeTranslatableStrings
        {
            get => _translationStrings;
            set => _translationStrings = value;
        }

        /// <summary>
        /// Specifies whether to include non-translatable strings
        /// </summary>
        public bool IncludeNonTranslatableStrings
        {
            get => _nonTranslationStrings;
            set => _nonTranslationStrings = value;
        }

        /// <summary>
        /// Parse a C# file and return all instances of requested strings
        /// </summary>
        /// <param name="filename">The file to parse</param>
        /// <returns>An enumeration of ParsedStringInfo</returns>
        public IEnumerable<ParsedPhrase> ParseFile(string filename)
        {
            Tokenizer tokenizer;
            using (var streamReader = new StreamReader(filename))
            {
                tokenizer = new Tokenizer(streamReader);

                // Scan for strings
                while (tokenizer.Token != Token.EOF)
                {
                    if (tokenizer.Token == Token.String)
                    {
                        string str = tokenizer.String;

                        if (!string.IsNullOrWhiteSpace(str))
                        {
                            int tokenLine = tokenizer.TokenLine;

                            // Should this string be included?
                            string context;
                            string comment;
                            if (Filter(out context, out comment))
                            {
                                // Yes, yield it
                                yield return new ParsedPhrase()
                                {
                                    Phrase = str,
                                    Context = context,
                                    Comment = comment,
                                    LineNumber = tokenLine,
                                };
                            }
                        }
                    }
                    tokenizer.NextToken();
                }
            }

            // Work out
            // Returns:
            // true if finds `.T()`
            // true if finds `.T("str")` with "str" returned via context
            bool IsTranslatableString(out string context, out string comment)
            {
                context = null;
                comment = null;
                tokenizer.NextToken();
                if (tokenizer.Token == Token.Period)
                {
                    tokenizer.NextToken();
                    if (tokenizer.Token == Token.Identifier && tokenizer.String == "T")
                    {
                        tokenizer.NextToken();
                        if (tokenizer.Token == Token.OpenRound)
                        {
                            tokenizer.NextToken();
                            if (tokenizer.Token == Token.String)
                            {
                                context = tokenizer.String;
                                tokenizer.NextToken();
                            }
                            if (tokenizer.Token == Token.Comment)
                            {
                                comment = tokenizer.String.Trim();
                                tokenizer.NextToken();
                            }
                            if (tokenizer.Token == Token.CloseRound)
                            {
                                tokenizer.NextToken();
                                return true;
                            }
                        }
                    }
                }
                return false;
            }

            // If looking for translation strings,
            //     returns true if .T() or .T("str") found
            // If looking for non-translation strings,
            //     returns true if neither .T() not .T("str") found
            // Otherwise returns false
            bool Filter(out string context, out string comment)
            {
                if (_translationStrings)
                    return IsTranslatableString(out context, out comment);
                if (_nonTranslationStrings && !IsTranslatableString(out context, out comment))
                    return true;
                context = null;
                comment = null;
                return false;
            }
        }


        bool _translationStrings = true;
        bool _nonTranslationStrings = false;
    }
}
