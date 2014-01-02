using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnnSharp.FaqMaster.Core.DnnSf.Actions
{
    public interface IAction
    {
        /// <summary>
        /// Initializes the interface using the settings from config and parameters specified under admin
        /// </summary>
        void Init(StringsDictionary settings, SettingsDictionary parameters);

        /// <summary>
        /// This is the call to execute the action.
        /// </summary>
        IActionResult Execute(IActionContext context);
    }
}
