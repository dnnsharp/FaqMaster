using DnnSharp.Common;
using DnnSharp.Common.Licensing.v1;
using DnnSharp.Common.Logging;
using DnnSharp.Common.Logging.Target;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace DnnSharp.FaqMaster.Core
{
    public class App
    {
        public static AppInfo Info { get; private set; }

        #region Environment

        /// <summary>
        /// The root file path where DNN is physically installed
        /// </summary>
        public static string RootPath { get { return Globals.ApplicationMapPath; } }

        /// <summary>
        /// The root file path where DNN is physically installed
        /// </summary>
        public static string RootUrl { get { return HttpRuntime.AppDomainAppVirtualPath.TrimEnd('/'); } }

        #endregion

        #region Singleton

        /// <summary>
        /// Singleton Instance
        /// </summary>
        public static App Instance { get; private set; }

        /// <summary>
        /// Singleton initialization
        /// </summary>
        static App()
        {
            Instance = new App();
        }

        #endregion

        #region Licensing and Registration

        public static bool IsAdmin { get { return DotNetNuke.Entities.Users.UserController.GetCurrentUserInfo().IsInRole(DotNetNuke.Entities.Portals.PortalController.GetCurrentPortalSettings().AdministratorRoleName); } }
        public static string RegCoreServer { get { return "http://www.dnnsharp.com/DesktopModules/RegCore/"; } }

        public List<System.Web.UI.WebControls.ListItem> Hosts
        {
            get
            {
                List<System.Web.UI.WebControls.ListItem> hosts = new List<System.Web.UI.WebControls.ListItem>();
                PortalAliasController paCtrl = new PortalAliasController();
                foreach (PortalAliasInfo paInfo in PortalAliasController.GetPortalAliasLookup().Values) {
                    hosts.Add(new System.Web.UI.WebControls.ListItem(paInfo.HTTPAlias, paInfo.HTTPAlias));
                }
                return hosts;
            }
        }

        internal IActivationDataStore GetActivationSrc()
        {
            return new DsLicFile();
        }

        public static IRegCoreClient RegCore
        {
            get
            {
                return RegCoreClient.Get(new RegCoreServer(RegCoreServer).ApiScript, App.Info.Code, new DsLicFile(), false);
            }
        }

        public static bool IsActivated()
        {
            return RegCore.IsActivated(App.Info.Code, App.Info.Version, HttpContext.Current.Request.Url.Host);
        }

        public static bool IsTrial()
        {
            return RegCore.IsTrial(App.Info.Code, App.Info.Version, HttpContext.Current.Request.Url.Host);
        }

        public static int TrialDaysLeft
        {
            get
            {
                ILicenseInfo act = RegCore.GetValidActivation(App.Info.Code, App.Info.Version, HttpContext.Current.Request.Url.Host);
                if (act != null)
                    return act.RegCode.DaysLeft;

                return -1;
            }
        }

        public static string CurrentRegistrationCode
        {
            get
            {
                ILicenseInfo act = RegCore.GetValidActivation(App.Info.Code, App.Info.Version, HttpContext.Current.Request.Url.Host);
                if (act != null)
                    return act.RegistrationCode;

                return "";
            }
        }

        public static bool IsTrialExpired()
        {
            return RegCore.IsTrialExpired(App.Info.Code, App.Info.Version, HttpContext.Current.Request.Url.Host);
        }

        public void Activate(string regCode, string host, string actCode)
        {
            if (string.IsNullOrEmpty(actCode)) {
                RegCore.Activate(regCode, App.Info.Code, App.Info.Version, host, App.Info.Key);
            } else {
                RegCore.Activate(regCode, App.Info.Code, App.Info.Version, host, App.Info.Key, actCode);
            }
        }

        #endregion


        #region Initialization

        public TypedLogger<FaqMasterSettings> Logger { get; set; }

        private App()
        {

            // app init
            Info = new AppInfo() {
                Name = "FAQ Master",
                Code = "FAQM",
                Key = "<RSAKeyValue><Modulus>rSJUC+DY1XNc4yApOiSK33gVqQy07ENpZAUnl+zVC8rsjd6sI9pEAr3ERmAin43CN6UQeHzQnU5qjqJWlSM0vIsSa5jGlzXhG0MsSLi+0wofoD87gzpfD6Zg3BdD8Ac50CnvfL5/zIFjx/9+f5V1q6RFh+tXY+s+IiOLRDpi1AM=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>",

                Version = System.Reflection.Assembly.GetAssembly(typeof(App)).GetName().Version.ToString(2) + ".0",
                Build = System.Reflection.Assembly.GetAssembly(typeof(App)).GetName().Version.ToString(3),

                BasePath = RootPath + "\\DesktopModules\\DnnSharp\\FaqMaster",
                BaseUrl = RootUrl + "/DesktopModules/DnnSharp/FaqMaster",
#if DEBUG
                IsDebug = true,
#else
                IsDebug = false,
#endif
            };
            // IsActivated depends on AppInfo, that's why we put it down here
            Info.IsActivated = App.IsActivated();

            // init container
            Container = new LiteContainer();
            Container.RegisterProperty("ConnectionString", () => DotNetNuke.Common.Utilities.Config.GetConnectionString());
            //Container.RegisterProperty("License", () => LicenseFactory.Get(LicenseFilePath, Version, ProductKey));

            // also populate the static AppContainer
            LiteContainer.AppContainer.RegisterProperty("RootUrl", () => App.RootUrl);
            LiteContainer.AppContainer.RegisterProperty("RootPath", () => App.RootPath);

            //Logger = new TypedLogger<FaqMasterSettings>();
            //Logger.FnLevel = (FaqMasterSettings data, eLogLevel currentMinLevel) => {
            //    return data == null || data.IsDebug.Value ? eLogLevel.Debug : eLogLevel.Info;
            //};

            //Logger.Targets.Add(new DnnTarget<FaqMasterSettings>());
            //Logger.Targets.Add(new SimpleFileTarget<FaqMasterSettings>(
            //    // fn to get file path
            //    (FaqMasterSettings data) => {
            //        var fileName = "log." + DateTime.Now.ToString("yyyy-MM-dd") + ".txt.resources";
            //        if (data == null)
            //            return string.Format("{0}\\Portals\\_default\\Logs\\ActionForm\\{1}", App.RootPath, fileName);

            //        var portalHome = new PortalSettings(data.PortalId).HomeDirectoryMapPath;
            //        return string.Format("{0}\\Logs\\ActionForm\\{1}", portalHome, fileName);
            //    },
            //    // fn to format message
            //    (eLogLevel level, FaqMasterSettings data, string message) => {
            //        return string.Format("{0} | {1} | {2} | {3}",
            //            DateTime.Now.ToString("HH:MM:ss"),
            //            level.ToString(),
            //            data == null ? "" : string.Format("{0} #{1}", data.Module.ModuleTitle, data.ModuleId),
            //            message);
            //    }));
        }

        #endregion


        //#region Licensing

        //static string LicensingUrl = "http://www.dnnsharp.com/DesktopModules/RegCore/Api.ashx?Method={0}&product=" + ProductCode + "&version=" + Version;
        //public static string BuyUrl = string.Format(LicensingUrl, "Buy");
        //public static string DocUrl = string.Format(LicensingUrl, "Doc");

        //public const string ProductName = "FAQ Master";
        //public const string ProductCode = "FAQM";
        //public const string ProductKey = "<RSAKeyValue><Modulus>rSJUC+DY1XNc4yApOiSK33gVqQy07ENpZAUnl+zVC8rsjd6sI9pEAr3ERmAin43CN6UQeHzQnU5qjqJWlSM0vIsSa5jGlzXhG0MsSLi+0wofoD87gzpfD6Zg3BdD8Ac50CnvfL5/zIFjx/9+f5V1q6RFh+tXY+s+IiOLRDpi1AM=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        //public static string Version
        //{
        //    get
        //    {
        //        var version = System.Reflection.Assembly.GetAssembly(typeof(App)).GetName().Version;
        //        return version.ToString(2);
        //    }
        //}

        //public static string Build
        //{
        //    get
        //    {
        //        var version = System.Reflection.Assembly.GetAssembly(typeof(App)).GetName().Version;
        //        return version.ToString(3);
        //    }
        //}

        //static string LicenseFilePath
        //{
        //    get
        //    {
        //        var asm = System.Reflection.Assembly.GetAssembly(typeof(App));
        //        var asmPath = asm.CodeBase.Replace("file:///", "").Replace('/', '\\');

        //        if (Path.GetExtension(asmPath).ToLower() == ".dll") {
        //            asmPath = Path.GetDirectoryName(asmPath);
        //        }

        //        if (asmPath.IndexOf(System.AppDomain.CurrentDomain.BaseDirectory.Replace('/', '\\')) == -1) {
        //            // it's not in the bin folder
        //            asmPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "bin");
        //        }

        //        return Path.Combine(asmPath, asm.GetName().Name.Replace(".Core", "") + ".lic");
        //    }
        //}

        //public LicenseBase License
        //{
        //    get { return Container.ResolveProperty("License") as LicenseBase; }
        //}

        //public bool IsActivated
        //{
        //    get { return License.Status.Type != LicenseStatus.eType.Error; }
        //}

        //#endregion


        const string MasterCacheKey = "DnnSharp.FaqMaster";
        public static string GetMasterCacheKey()
        {
            if (HttpRuntime.Cache[MasterCacheKey] != null)
                HttpRuntime.Cache.Insert(MasterCacheKey, new object());

            return MasterCacheKey;
        }

        public LiteContainer Container { get; private set; }

    }
}
