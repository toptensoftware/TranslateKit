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
        public string Phrase { get; set; }

        [Json("context", ExcludeIfNull = true)]
        public string Context { get; set; }

        [Json("comment", ExcludeIfNull = true)]
        public string Comment { get; set; }

        [Json("locations", ExcludeIfEmpty = true)]
        public List<string> Locations;

        [Json("translation")]
        public string Translation { get; set; }

        [Json("machine", ExcludeIfEquals = false)]
        public bool Machine { get; set; }

    }
}
