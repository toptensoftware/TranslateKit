using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Topten.JsonKit;

namespace TranslateTool
{
    public class PhraseContext
    {
        [Json("locations")]
        public List<string> Locations;

        [Json("translation")]
        public string Translation;

        [Json("machine")]
        public bool Machine;
    }

    public class Phrase
    {
        [Json("locations")] 
        public List<string> Locations;

        [Json("contexts")]
        public Dictionary<string, PhraseContext> Contexts;

        [Json("translation")] 
        public string Translation;

        [Json("machine")] 
        public bool Machine;
    }
}
