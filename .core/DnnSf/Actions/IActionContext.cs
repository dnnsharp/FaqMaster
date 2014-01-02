using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnnSharp.FaqMaster.Core.DnnSf.Actions
{
    public interface IActionContext
    {
        SettingsDictionary Data { get; }

        HashSet<int> Users { get; set; }

        PortalInfo Portal { get; set; }

        TabInfo Page { get; set; }

        ModuleInfo Module { get; set; }

        object this[string key] { get; set; }

        string ApplyAllTokens(string input, UserInfo user = null, ModuleInfo module = null, TokenUtil.eContext context = TokenUtil.eContext.None);
    }
}
