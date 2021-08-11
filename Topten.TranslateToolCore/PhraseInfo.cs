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

        [Json("locations", ExcludeIfNull = true)]
        public List<string> Locations_null_check
        {
            get
            {
                if (Locations == null)
                    return null;
                if (Locations.Count == 0)
                    return null;
                return Locations;
            }
            set
            {
                Locations = value;
            }
        }

        public List<string> Locations;

        [Json("translation")]
        public string Translation { get; set; }

        [Json("machine", ExcludeIfNull = true)]
        public bool? Machine_null_check
        {
            get => Machine == false ? null : true;
            set => Machine = value.Value;
        }

        public bool Machine { get; set; }

    }
}
