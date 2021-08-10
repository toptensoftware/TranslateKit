using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topten.JsonKit;

namespace Topten.TranslateKit
{
    /// <summary>
    /// Helper for creating a language translator function
    /// </summary>
    public static class Translator
    {
        /// <summary>
        /// Given a path to a json file creates a translator function
        /// </summary>
        /// <param name="jsonFile">The JSON file containing the translations</param>
        /// <returns>A TranslateFunc that can be assigned to StringTranslationExtension.Translate</returns>
        public static TranslateFunc CreateTranslator(string jsonFile)
        {
            // Load the phrase map
            Dictionary<string, Phrase> phraseMap = Json.ParseFile<Dictionary<string, Phrase>>(jsonFile);

            return (phrase, context) =>
            {
                // Look up the phrase
                if (!phraseMap.TryGetValue(phrase, out var phraseEntry))
                    return phrase;

                // Look for context version
                if (context != null)
                {
                    if (phraseEntry.Contexts.TryGetValue(context, out var contextEntry))
                    {
                        if (contextEntry.Translation != null)
                            return contextEntry.Translation;
                    }
                }

                // Is there a translation?
                if (phraseEntry.Translation != null)
                    return phraseEntry.Translation;

                // Use original string
                return phrase;
            };
        }
    }
}
