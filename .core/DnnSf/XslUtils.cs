using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DnnSharp.FaqMaster.Core.DnnSf
{
    public class XslUtils
    {
        public string ResourceFile { get; set; }

        public XslUtils()
        {

        }

        public XslUtils(string localeFile)
        {
            ResourceFile = localeFile;
        }

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


        public string Localize(string key, string defaultText = null)
        {
            var text = DotNetNuke.Services.Localization.Localization.GetString(key, ResourceFile);
            if (string.IsNullOrEmpty(text))
                text = defaultText;
            return text;
        }

        /// <summary>
        /// Also have it lowercase so it's most like XSL standards
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public string localize(string key, string defaultText = null)
        {
            return Localize(key, defaultText);
        }

        /// <summary>
        /// Replace all [FieldName] patterns with form.fields.FieldName
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public string ParseAngularJs(string expression)
        {
            return Tokenize(Regex.Replace(expression, "\\[([^\\]:]+)\\]", "form.fields.$1.value", RegexOptions.IgnoreCase)
                .Replace("\"", "\'"));
        }

        public string parseAngularJs(string expression)
        {
            return ParseAngularJs(expression);
        }

    }
}
