using DnnSharp.Common;
using DnnSharp.Common.Api;
using DnnSharp.Common.Dnn;
using DnnSharp.Common.Licensing.v3;
using DnnSharp.Common.Logging;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;

namespace DnnSharp.FaqMaster.Core.Services
{
    //TODO: maybe an attribute to take care of security??

    /// <summary>
    /// This file exists here because .ashx files do not support CodeFile attribute
    /// </summary>
    public class AdminApi : IHttpHandler, IRequiresSessionState
    {
        #region IHttpHandler Members

        public bool IsReusable
        {
            // Return false in case your Managed Handler cannot be reused for another request.
            // Usually this would be false in case you have some state information preserved per request.
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            // extract some common variables
            var portal = PortalControllerEx.GetCurrentPortal(context);
            var moduleId = ConvertUtils.Cast<int>(context.Request.QueryString["mid"], -1);

            // register the API and some dependency injection
            var api = new ApiContext();
            api.Container.RegisterProperty("portalAlias", () => portal.PortalAlias);
            api.Container.RegisterProperty("portalSettings", () => portal);
            api.Container.RegisterProperty("portalId", () => portal == null ? -1 : portal.PortalId);
            api.Container.RegisterProperty("locale", () => portal == null ? Thread.CurrentThread.CurrentCulture.ToString() : portal.CultureCode);
            api.Container.RegisterProperty("moduleInfo", () => new ModuleController().GetModule(moduleId));
            api.Container.RegisterProperty("moduleId", () => moduleId);

            var config = moduleId == -1 ? null : new FaqMasterSettings().Load(moduleId);
            api.Container.RegisterProperty("config", () => config);

            api.Execute(this, context, (eLogLevel level, string message) => {
                App.Instance.Logger.Log(level, config, message);
            });
        }

        #endregion


        #region Licensing

        [WebMethod(DefaultResponseType = eResponseType.Json, RequiredEditPermissions = true)]
        public void SaveActivation()
        {
            var json = new StreamReader(HttpContext.Current.Request.InputStream).ReadToEnd();
            var newLics = new JavaScriptSerializer().Deserialize<IEnumerable<LicenseInfo>>(json);
            new RegCoreClient().AddActivations(newLics);
        }

        [WebMethod(DefaultResponseType = eResponseType.Json, RequiredEditPermissions = true)]
        public DnnSharp.Common.Licensing.v3.LicenseStatus GetLicenseStatus(PortalSettings portalSettings)
        {
            var status = new DnnSharp.Common.Licensing.v3.RegCoreClient().IsActivated(App.Info);
            status.ActivationUrl = App.GetActivationUrl(portalSettings);
            return status;
        }

        #endregion

    }
}
