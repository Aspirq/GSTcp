using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormulaLib
{
    public static class Extension
    {
        public static string[] Explode(this string source, string separator)
        {
            return source.Split(new string[] { separator }, StringSplitOptions.None);
        }

        public static Boolean IsAnyOf(this Char source, Char[] chars)
        {
            for (int i = 0; i < chars.Count(); i++)
            {
                if (source == chars[i]) { return true; }
            }
            return false;
        }
    }
}
