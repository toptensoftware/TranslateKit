using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslateTool
{
    class CommandLineUtils
    {
        public static bool IsSwitch(string arg, out string switchName, out string switchValue)
        {
            // Args are in format [/--]<switchname>[:<value>];
            if (arg.StartsWith("/") || arg.StartsWith("-"))
            {
                // Split into switch name and value
                switchName = arg.Substring(arg.StartsWith("--") ? 2 : 1);
                switchValue = null;
                int colonpos = switchName.IndexOf(':');
                if (colonpos >= 0)
                {
                    switchValue = switchName.Substring(colonpos + 1);
                    switchName = switchName.Substring(0, colonpos).ToLower();
                }
                return true;
            }
            else
            {
                switchValue = null;
                switchName = null;
                return false;
            }
        }
    }
}
