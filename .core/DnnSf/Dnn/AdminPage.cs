using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace DnnSharp.FaqMaster.Core.DnnSf.Dnn
{
    /// <summary>
    /// This is the base class for admin aspx pages
    /// </summary>
    public class AdminPage : System.Web.UI.Page
    {
        protected PortalAliasInfo PortalAlias { get; set; }
        protected PortalSettings Portal { get; set; }
        protected TabInfo Tab { get; set; }
        protected ModuleInfo Module { get; set; }
        protected UserInfo DnnUser { get; set; }
        protected string StrAlias { get; set; }
        protected string CommPrefix { get; set; }

        protected string UnlockTrialUrl { get; set; }
        protected string ActivateUrl { get; set; }
        protected string ViewUrl { get; set; }

        protected virtual Boolean IsDebug { get { return false; } }

        protected void InitAdmin(Type controller)
        {
            if (string.IsNullOrEmpty(Request.QueryString["alias"])) {
                throw new Exception("Invalid portal alias.");
            }

            Portal = PortalControllerEx.GetCurrentPortal(Request.QueryString["alias"]);
            if (Portal == null)
                throw new Exception("Invalid portal " + PortalAlias.PortalID);

            PortalAlias = Portal.PortalAlias;
            if (PortalAlias == null)
                throw new Exception("Invalid portal alias " + Request.QueryString["alias"]);

            int tabId; // = ParseInt(Request.QueryString["_tabId"], "Invalid tab ID {0}");
            if (Request.QueryString["_tabId"] != null && int.TryParse(Request.QueryString["_tabId"], out tabId))
                Tab = new TabController().GetTab(tabId, Portal.PortalId, false);
            //if (Tab == null)
            //    throw new Exception("Invalid tab " + tabId);

            int modId; // = ParseInt(Request.QueryString["_mid"], "Invalid module ID {0}");
            if (Tab != null && Request.QueryString["_mid"] != null && int.TryParse(Request.QueryString["_mid"], out modId))
                Module = new ModuleController().GetModule(modId, Tab.TabID);
            //if (Module == null)
            //    throw new Exception("Invalid module " + modId);

            StrAlias = PortalControllerEx.SanitizePortalAliasForQueryString(Request.Url, PortalAlias);
            CommPrefix = Request.QueryString["comm-prefix"];

            DnnUser = UserController.GetCurrentUserInfo();

            if ((Module != null && !ModulePermissionController.CanEditModuleContent(Module)) || (Module == null && !DnnUser.IsInRole(Portal.AdministratorRoleName))) {
                // redirect to access denied page/login
                //Page.ClientScript.RegisterClientScriptBlock(GetType(), "access-denied", "top.location = '" + DotNetNuke.Common.Globals.AccessDeniedURL() +"';", true);
                Response.Write("Access Denied!");
                Response.End();
            }

            // common URLs
            if (!string.IsNullOrEmpty(Request.QueryString["returnurl"])) {
                ViewUrl = Request.QueryString["returnurl"];
            } else if (Tab != null) {
                ViewUrl = DotNetNuke.Common.Globals.NavigateURL(Tab.TabID);
            } else {
                ViewUrl = App.RootUrl;
            }

            UnlockTrialUrl = TemplateSourceDirectory + "/RegCore/UnlockTrial.aspx?t=" + HttpUtility.UrlEncode(controller.AssemblyQualifiedName) + "&rurl=" + HttpUtility.UrlEncode(Request.RawUrl + "#refresh");
            ActivateUrl = TemplateSourceDirectory + "/RegCore/Activation.aspx?t=" + HttpUtility.UrlEncode(controller.AssemblyQualifiedName) + "&rurl=" + HttpUtility.UrlEncode(Request.RawUrl + "#refresh");

            // serialize constant data right from the start to save requests
            RegisterJson("g_dnnsfState", GetAppStateJson());
        }

        protected int ParseInt(string s, string error)
        {
            int val;
            if (string.IsNullOrEmpty(s))
                throw new Exception(string.Format(error, ""));

            if (!int.TryParse(s, out val))
                throw new Exception(string.Format(error, s));

            return val;
        }

        protected void RegisterJson(string varName, object obj)
        {
            string json = new JavaScriptSerializer().Serialize(obj);
            Page.ClientScript.RegisterClientScriptBlock(GetType(), "init" + varName, "var " + varName + " = " + json + ";", true);
        }

        Dictionary<string, object> GetAppStateJson()
        {
            var url = new DnnUrl(Request.Url);
            url.QueryString.Remove("mode");

            var state = new Dictionary<string, object>();
            state["adminApi"] = App.BaseUrl + "/AdminApi.ashx";
            state["appUrl"] = App.BaseUrl;
            state["isDebug"] = IsDebug || App.IsDebug;
            state["isIframe"] = Request.QueryString["mode"] == "iframe";
            state["adminUrl"] = url.Build(UriKind.Relative);
            state["viewUrl"] = Tab == null ? "/" : Globals.NavigateURL(Tab.TabID);
            state["alias"] = StrAlias;
            state["moduleId"] = Module == null ? -1 : Module.ModuleID;
            state["version"] = App.Build;
            state["protocol"] = HttpContext.Current.Request.Url.Scheme;

            //// TODO: export licensing info
            //state["licensing"] = new Dictionary<string, object>() {
            //    { "activations", "" }
            //};
            return state;
        }
    }
}
