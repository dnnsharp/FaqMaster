using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnnSharp.FaqMaster.Core.DnnSf
{
    public static class SecurityUtil
    {
        /// <summary>
        /// Prevent  attacks such as ../../web.config or worse
        /// </summary>
        /// <returns></returns>
        public static string SecureFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return filePath;

            filePath = filePath.Replace('/', '\\');
            return filePath.Replace("..\\", "");
        }
    }
}
