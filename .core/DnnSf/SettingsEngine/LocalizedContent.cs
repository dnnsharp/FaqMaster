using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnnSharp.FaqMaster.Core.DnnSf.SettingsEngine
{
    public class LocalizedContent
    {
        /// <summary>
        /// Using this key we can hook to Asp.NET localization
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Fallback if no localization can be found
        /// </summary>
        public string Default { get; set; }

        /// <summary>
        /// It's possible to also provide localization via config files
        /// but Asp.NET localization will override it
        /// </summary>
        public IDictionary<string, string> Localization { get; set; }
    }
}
