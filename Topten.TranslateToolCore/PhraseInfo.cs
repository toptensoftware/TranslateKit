using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Topten.JsonKit;

namespace TranslateTool
{
    public class PhraseInfo
    {
        [Json("phrase")]
        public string Phrase;

        [Json("context")]
        public string Context;

        [Json("comment")]
        public string Comment;

        [Json("locations")]
        public List<string> Locations;

        [Json("translation")]
        public string Translation;

        [Json("machine")]
        public bool Machine;

    }
}
