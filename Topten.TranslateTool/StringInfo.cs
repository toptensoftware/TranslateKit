using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslateTool
{
    public class StringInfo
    {
        public StringInfo(string str)
        {
            this.str = str;
            locations = new List<string>();
            contexts = new Dictionary<string, List<string>>();
        }
        // The original string
        public string str;
        public List<string> locations;
        public Dictionary<string, List<string>> contexts;
    }
}
