using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace DnnSharp.FaqMaster.Core.DnnSf.Dnn
{
    /// <summary>
    /// This is a base class for the view that hosts the admin aspx page
    /// </summary>
    public class DnnView : PortalModuleBase
    {
        protected void InitView()
        {

        }

        protected string ManageUrl(string name)
        {
            return string.Format("{0}/{1}?alias={2}&_mid={3}&_tabid={4}",
                TemplateSourceDirectory, name, HttpUtility.UrlEncode(PortalAlias.HTTPAlias.ToLower()), ModuleId, TabId);
        }

    }
}
