using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DnnSharp.FaqMaster.Core.DnnSf
{
    public static class RegexUtil
    {
        public static bool IsEmail(string content)
        {
            return Regex.IsMatch(content, "^[A-Za-z0-9_\\+-]+(\\.[A-Za-z0-9_\\+-]+)*@[A-Za-z0-9-]+(\\.[A-Za-z0-9-]+)*\\.([A-Za-z]{2,4})$");
        }
    }
}
