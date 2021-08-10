using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topten.JsonKit;

namespace Topten.TranslateKit
{
    /// <summary>
    /// Represents a phrase context in the JSON file (runtime only properties)
    /// </summary>
    public class PhraseContext
    {
        /// <summary>
        /// The translation for this string
        /// </summary>
        [Json("translation")]
        public string Translation;
    }

    /// <summary>
    /// Represents a phrase in the JSON file (runtime only properties)
    /// </summary>
    public class Phrase
    {
        /// <summary>
        /// Context variations for this phrase
        /// </summary>
        [Json("contexts")]
        public Dictionary<string, PhraseContext> Contexts;

        /// <summary>
        /// The root translation of this phrase
        /// </summary>
        [Json("translation")]
        public string Translation;
    }
}
