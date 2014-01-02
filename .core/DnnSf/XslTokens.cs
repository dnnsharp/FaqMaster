using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnnSharp.FaqMaster.Core.DnnSf
{
    public class XslTokens
    {
        public string Replace(string content)
        {
            return TokenUtil.Tokenize(content, null, null, false, true);
        }

        /// <summary>
        /// Also have it lowercase so it's most like XSL standards
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public string replace(string content)
        {
            return TokenUtil.Tokenize(content, null, null, false, true);
        }

        public string Tokenize(string content)
        {
            return TokenUtil.Tokenize(content, null, null, false, true);
        }

        /// <summary>
        /// Also have it lowercase so it's most like XSL standards
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public string tokenize(string content)
        {
            return TokenUtil.Tokenize(content, null, null, false, true);
        }

    }
}
