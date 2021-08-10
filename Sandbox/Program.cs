using System;
using System.Reflection;
using Topten.TranslateKit;

namespace Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            // Work out where the strings file is
            var baseDir = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var stringsFile = System.IO.Path.Combine(baseDir, "strings-fr.json");

            // Setup translator
            StringTranslationExtensions.Translate = Translator.CreateTranslator(stringsFile);

            // Do stuff...
            Console.WriteLine("Hello World!".T());

            Console.WriteLine("Block".T("obstruct"));
            Console.WriteLine("Block".T("cube"));
        }
    }
}
