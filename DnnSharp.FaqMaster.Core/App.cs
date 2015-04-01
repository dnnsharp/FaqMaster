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

        #region Licensing


        public static string Version
        {
            get
            {
                var version = System.Reflection.Assembly.GetAssembly(typeof(App)).GetName().Version;
                return version.ToString(2);
            }
        }

        public static string Build
        {
            get
            {
                var version = System.Reflection.Assembly.GetAssembly(typeof(App)).GetName().Version;
                return version.ToString(3);
            }
        }

        static string LicenseFilePath
        {
            get
            {
                var asm = System.Reflection.Assembly.GetAssembly(typeof(App));
                var asmPath = asm.CodeBase.Replace("file:///", "").Replace('/', '\\');

                if (Path.GetExtension(asmPath).ToLower() == ".dll") {
                    asmPath = Path.GetDirectoryName(asmPath);
                }

                if (asmPath.IndexOf(System.AppDomain.CurrentDomain.BaseDirectory.Replace('/', '\\')) == -1) {
                    // it's not in the bin folder
                    asmPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "bin");
                }

                return Path.Combine(asmPath, asm.GetName().Name.Replace(".Core", "") + ".lic");
            }
        }

        //public LicenseBase License
        //{
        //    get { return Container.ResolveProperty("License") as LicenseBase; }
        //}

        public static bool IsActivated()
        {
            var status = new DnnSharp.Common.Licensing.v3.RegCoreClient().IsActivated(Info);
            return status.Code == DnnSharp.Common.Licensing.v3.LicenseStatus.eCode.Ok;
            //return RegCore.IsActivated(App.Info.Code, App.Info.Version, HttpContext.Current.Request.Url.Host, false);
        }

        public static string GetActivationUrl(PortalSettings portal)
        {
            return App.Info.BaseUrl + "/RegCore/Activate.aspx?_alias=" + HttpUtility.UrlEncode(portal.PortalAlias.HTTPAlias)
            + "&returnurl=" + HttpUtility.UrlEncode(HttpContext.Current.Request.RawUrl);
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
            UpdateCommon("DnnSharp.Common.dll");

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

        void UpdateCommon(string name)
        {
#if !DEBUG
            try {
                var backupDir = Path.Combine(App.Info.BasePath, "backup");
                if (!Directory.Exists(backupDir))
                    Directory.CreateDirectory(backupDir);

                var backup = Path.Combine(backupDir, name);
                var current = Path.Combine(App.RootPath, "bin\\" + name);

                var asmBackup = File.Exists(backup) ? System.Reflection.AssemblyName.GetAssemblyName(backup) : null;
                var asmCurrent = File.Exists(current) ? System.Reflection.AssemblyName.GetAssemblyName(current) : null;

                var touched = false;
                if (asmBackup != null && (asmCurrent == null || asmBackup.Version > asmCurrent.Version)) {
                    File.Copy(backup, current, true);
                    touched = true;
                } else if (asmCurrent != null && (asmBackup == null || asmBackup.Version < asmCurrent.Version)) {
                    File.Copy(current, backup, true);
                    touched = true;
                }

                if (touched && HttpContext.Current != null)
                    HttpContext.Current.Response.Redirect(HttpContext.Current.Request.RawUrl, true);

            } catch { }
#endif
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
