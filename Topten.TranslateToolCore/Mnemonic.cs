using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TranslateTool
{
    public static class Mnemonic
    {
        public static string CleanText(string text)
        {
            if (text == null)
                return text;

            var index = text.IndexOf('&');
            if (index < 0)
                return text;

            var sb = new StringBuilder(text.Substring(0, index));
            for (int i = index; i < text.Length; i++)
            {
                if (text[i] == '&')
                {
                    if (i + 1 < text.Length)
                    {
                        if (text[i+1] == '&')
                        {
                            sb.Append('&');
                            i++;
                        }
                    }
                }
                else
                {
                    sb.Append(text[i]);
                }
            }

            return sb.ToString();
        }

        public static char FromText(string text)
        {
            if (text == null)
                return '\0';

            text = text.ToUpper();

            int startIndex = 0;
            while (true)
            {
                int amperPos = text.IndexOf('&', startIndex);
                if (amperPos < 0)
                    return '\0';
                if (amperPos + 1 >= text.Length)
                    return '\0';

                if (text[amperPos + 1] == '&')
                    startIndex = amperPos + 2;
                else
                    return text[amperPos + 1];
            }
        }
    }
}
