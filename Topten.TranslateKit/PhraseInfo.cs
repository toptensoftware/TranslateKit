using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topten.JsonKit;

namespace Topten.TranslateKit
{
    /// <summary>
    /// Represents a phrase in the JSON file (runtime only properties)
    /// </summary>
    public class PhraseInfo
    {
        /// <summary>
        /// The original phrase
        /// </summary>
        [Json("phrase")]
        public string Phrase;

        /// <summary>
        /// The phrase context
        /// </summary>
        [Json("context")]
        public string Context;

        /// <summary>
        /// The translation of this phrase
        /// </summary>
        [Json("translation")]
        public string Translation;
    }
}
