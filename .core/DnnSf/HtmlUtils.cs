using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace DnnSharp.FaqMaster.Core.DnnSf
{
    public class HtmlUtils
    {
        public string StripHtml(string content)
        {
            return Regex.Replace(HttpUtility.HtmlDecode(content), @"<(.|\n)*?>", string.Empty);
        }

        /// <summary>
        /// Also have it lowercase so it's most like XSL standards
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public string stripHtml(string content)
        {
            return StripHtml(content);
        }

        public string EncodeHtml(string content)
        {
            return HttpUtility.HtmlEncode(content);
        }

        /// <summary>
        /// Also have it lowercase so it's most like XSL standards
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public string encodeHtml(string content)
        {
            return EncodeHtml(content);
        }

        public string DecodeHtml(string content)
        {
            return HttpUtility.HtmlDecode(content);
        }

        /// <summary>
        /// Also have it lowercase so it's most like XSL standards
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public string decodeHtml(string content)
        {
            return DecodeHtml(content);
        }
    }
}
