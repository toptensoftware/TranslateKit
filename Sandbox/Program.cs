using System;
using System.Reflection;
using Topten.TranslateKit;

namespace Sandbox
{
    class MyAttribute : Attribute
    {
        public MyAttribute(string a, string b)
        {
        }
    }
    class Program
    {
        [My("apples", @"Apples")]
        static void Main(string[] args)
        {
            // Work out where the strings file is
            var baseDir = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var stringsFile = System.IO.Path.Combine(baseDir, "strings-fr.json");

            // Setup translator
            StringTranslationExtensions.Translate = Translator.CreateTranslator(stringsFile);

            // Do stuff...
            Console.WriteLine("Hello World!".T());

            Console.WriteLine("Block".T("obstruct" /* as in "obstruct" */));
            Console.WriteLine("Block".T("cube"));
            Console.WriteLine("Block".T("cube" /* as in "a brick" */));
        }
    }
}
