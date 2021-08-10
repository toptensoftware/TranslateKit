using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Topten.JsonKit;

namespace TranslateTool
{
    public static class GoogleTranslate
    {
        public static string Translate(string phrase, string targetLanguage)
        {
            var lines = phrase.Split('\n').ToArray();

            for (int i = 0; i < lines.Length; i++)
                lines[i] = TranslateLine(lines[i], targetLanguage);

            return string.Join("\n", lines);
        }

        public static string TranslateLine(string phrase, string targetLanguage)
        {
            var strLeading = "";
            var strTrailing = "";

            // Remove leading characters
            while (phrase.Length > 0 && IsTrimChar(phrase[0]))
            {
                strLeading += phrase[0];
                phrase = phrase.Substring(1);
            }

            // Remove trailing characters
            while (phrase.Length > 0 && IsTrimChar(phrase[phrase.Length - 1]))
            {
                strTrailing = phrase[phrase.Length - 1] + strTrailing;
                phrase = phrase.Substring(0, phrase.Length - 1);
            }

            // Remove and remember mnemonic
            char chMnemonic = Mnemonic.FromText(phrase);
            phrase = Mnemonic.CleanText(phrase);

            // Fix up "&&"
            phrase = phrase.Replace("&&", "&");

            // De-proper case it (google translate doesn't like caps)
            phrase = DeProperCase(phrase);

            // Extract format strings
            var formatStrings = ExtractFormatStrings(ref phrase);


            // Do The Translation
            phrase = TranslateRaw(phrase, targetLanguage);


            // Replace format strings
            ReplaceFormatStrings(ref phrase, formatStrings);

            // Put back ampersands
            phrase = phrase.Replace("&", "&&");

            // Put back the mnemonic
            if (chMnemonic != '\0')
            {
                int index = phrase.ToLower().IndexOf(char.ToLower(chMnemonic));
                if (index >= 0)
                {
                    phrase = phrase.Substring(0, index) + "&" + phrase.Substring(index);
                }
                else
                {
                    phrase += string.Format(" (&{0})", char.ToUpper(chMnemonic));
                }
            }

            // Put back leading trailing
            phrase = strLeading + phrase + strTrailing;

            // Fix up crazy spacing
            phrase = phrase.Replace(" .", ".");
            phrase = phrase.Replace(" :", ":");
            phrase = phrase.Replace("( ", "(");
            phrase = phrase.Replace(" )", ")");
            phrase = phrase.Replace(" ,", ",");

            return phrase;
        }

        public static string ApiKey;

        public static string TranslateRaw(string str, string target)
        {
            if (ApiKey == null)
                throw new InvalidOperationException("Google Translate API key not set");

            var urlEncoded = HttpUtility.UrlEncode(str);
            var url = string.Format("https://www.googleapis.com/language/translate/v2?key={2}&source=en&target={0}&q={1}", target, urlEncoded, ApiKey);
            Console.Write("Translating \"{0}\" -> ", str);
            var strJson = HttpUtils.GetJson(url);
            var json = Json.Parse<Dictionary<string, object>>(strJson);

            var translations = json.GetPath<object[]>("data.translations");
            var translation = translations[0] as IDictionary<string, object>;
            var text = translation.GetPath<string>("translatedText");

            text = HttpUtility.HtmlDecode(text);

            Console.WriteLine("\"{0}\"", text);

            return text;
        }

        static bool IsTrimChar(char ch)
        {
            if (ch == '{' || ch == '}' || ch=='&')
                return false;
            return char.IsWhiteSpace(ch) || char.IsPunctuation(ch);
        }

        static string DeProperCase(string str)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < str.Length; i++)
            {
                if (char.IsUpper(str[0]) &&
                    i > 0 &&
                    !char.IsLetter(str[i - 1]) &&
                    i + 1 < str.Length &&
                    !char.IsUpper(str[i + 1]))
                {
                    sb.Append(char.ToLower(str[i]));
                }
                else
                {
                    sb.Append(str[i]);
                }
            }

            return sb.ToString();
        }

        public static List<string> ExtractFormatStrings(ref string str)
        {
            if (!str.Contains('{'))
                return null;

            var sb = new StringBuilder();
            var list = new List<string>();

            bool inEscape = false;
            int escapePos = 0;
            for (int i=0; i<str.Length; i++)
            {
                if (inEscape)
                {
                    if (str[i] == '}')
                    {
                        inEscape = false;
                        list.Add(str.Substring(escapePos, i - escapePos + 1));
                    }
                }
                else
                {
                    if (str[i] == '{')
                    {
                        escapePos = i;
                        inEscape = true;
                        sb.Append(string.Format("ZZ{0}", list.Count));
                    }
                    else
                    {
                        sb.Append(str[i]);
                    }
                }
            }

            str = sb.ToString();
            return list;
        }

        public static void ReplaceFormatStrings(ref string str, List<string> formatStrings)
        {
            if (formatStrings == null)
                return;

            for (int i = 0; i < formatStrings.Count; i++)
            {
                str = str.Replace(string.Format("ZZ{0}", i), formatStrings[i]);
            }
        }
    }
}
