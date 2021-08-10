using System;

namespace Topten.TranslateKit
{
    /// <summary>
    /// Delegate type for translate callback function
    /// </summary>
    /// <param name="phrase"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public delegate string TranslateFunc(string phrase, string context);


    /// <summary>
    /// Provides the .T() extension for translatable strings
    /// </summary>
    public static class StringTranslationExtensions
    {
        /// <summary>
        /// Extension method for translatable strings
        /// </summary>
        /// <param name="This">The string to be translated</param>
        /// <param name="context">An optional context for the translation</param>
        /// <returns>The translated string</returns>
        public static string T(this string This, string context = null)
        {
            if (Translate == null)
                return null;
            return Translate(This, context);
        }

        /// <summary>
        /// Plugin function to provide translations for a phrase
        /// </summary>
        public static TranslateFunc Translate = (phrase, context) => phrase;
    }
}
