using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Permissions;
using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace DnnSharp.FaqMaster.Core.DnnSf.Dnn
{
    /// <summary>
    /// This is a base class for the view that hosts the admin aspx page
    /// </summary>
    public class AdminControl : PortalModuleBase
    {
        protected string StrAlias { get; set; }
        protected bool CanAdmin { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            // set CanAdmin before calling base.OnLoad, since that in turns cals Page_Load for our control
            CanAdmin = UserInfo != null && UserInfo.UserID != -1 && ModulePermissionController.CanEditModuleContent(ModuleConfiguration);

            base.OnLoad(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="page">The aspx page to render inside iframe</param>
        /// <param name="version">The version is displayed under the iframe</param>
        /// <param name="optUrl">The Open Traits URL to embed the widget. Pass null to hide the widget.</param>
        protected void InitAdmin(string page, string version, string optUrl = null)
        {
            ModuleConfiguration.ModuleControl.ControlTitle = new ModuleController().GetModule(this.ModuleId, this.TabId).ModuleTitle;

            StrAlias = PortalControllerEx.SanitizePortalAliasForQueryString(Request.Url, PortalSettings.PortalAlias);

            //  be sure to have jQuery for the message loop
            DotNetNuke.Framework.jQuery.RequestRegistration();

            // The prefix is used for communications between iframe and page, 
            // so if there are multiple modules of different kinds they don't interfere</param>
            var prefix = Guid.NewGuid().ToString().Substring(0, 9).Replace("-", "");

            // create a new iframe
            var cIframe = new HtmlGenericControl("iframe");
            cIframe.Attributes["class"] = "dnnsf-iframe dnnsf-iframe-admin";
            cIframe.Attributes["style"] = "width: 100%; height: 700px; border: 1px solid #ccc; min-width: 770px;";
            cIframe.Attributes["src"] = string.Format("{0}/{1}?alias={2}&_mid={3}&_tabid={4}&mode=iframe&comm-prefix={5}",
                TemplateSourceDirectory, page, HttpUtility.UrlEncode(StrAlias), ModuleId, TabId, prefix);
            this.Controls.Add(cIframe);

            // also append the version number
            var cVersion = new HtmlGenericControl("p");
            cVersion.Attributes["class"] = "dnnsf-version";
            cVersion.Attributes["style"] = "font-size: 0.85em; color: #888; margin: 2px 10px;";
            cVersion.InnerText = string.Format("Version {0}", version);
            this.Controls.Add(cVersion);

            // initialization javascript
            if (!Page.ClientScript.IsClientScriptIncludeRegistered("dnnsfjQuery"))
                Page.ClientScript.RegisterClientScriptInclude("dnnsfjQuery", TemplateSourceDirectory + "/static/jquery.min.js?v=" + version);

            if (!Page.ClientScript.IsClientScriptIncludeRegistered("dnnsfAdminContainer"))
                Page.ClientScript.RegisterClientScriptInclude("dnnsfAdminContainer", TemplateSourceDirectory + "/static/dnnsf/admin.container.js?v=" + version);

            var initCall = string.Format("dnnsfInitFrame(dnnsfjQuery('#{0}'), '{1}', '{2}');", cIframe.ClientID, prefix, optUrl ?? "");
            Page.ClientScript.RegisterClientScriptBlock(GetType(), "", "dnnsfjQuery(function() { " + initCall + " });", true);

        }

    }
}
