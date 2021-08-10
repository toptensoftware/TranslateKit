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
            var phraseList = Json.ParseFile<List<PhraseInfo>>(jsonFile);
            var phraseMap = phraseList.ToDictionary(
                x => (x.Phrase, x.Context ?? ""),
                x => x
                );
            phraseList = null;

            return (phrase, context) =>
            {
                // Look up the phrase
                if (phraseMap.TryGetValue((phrase, context??""), out var pi))
                    return pi.Translation;

                // Use original string
                return phrase;
            };
        }
    }
}
