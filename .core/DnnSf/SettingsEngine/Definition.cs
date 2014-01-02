using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnnSharp.FaqMaster.Core.DnnSf.SettingsEngine
{
    public class Definition
    {
        /// <summary>
        /// The ID is used as the key in the dictionary that stores t
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// This is used for display purposes
        /// </summary>
        public LocalizedContent Title { get; set; }

        /// <summary>
        /// This is the help string displayed next to the setting
        /// </summary>
        public LocalizedContent HelpText { get; set; }

        /// <summary>
        /// This is a contract between the config and the Settings Engine
        /// We're just transporting it.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// This is a .NET type that implements the logic for this defintion
        /// </summary>
        public string TypeImplementation { get; set; }

        /// <summary>
        /// This is a dictionary that's passed to the SettingsEngine and to TypeImplementation
        /// </summary>
        public IDictionary<string, object> Settings { get; set; }

        /// <summary>
        /// A Definition could contain subdefinitions for parameters
        /// </summary>
        public IList<Definition> Parameters { get; set; }

        public Definition()
        {
            Settings = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            Parameters = new List<Definition>();
        }

    }
}
