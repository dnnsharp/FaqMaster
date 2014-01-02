using DnnSharp.FaqMaster.Core.DnnSf.Dnn;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Localization;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;

namespace DnnSharp.FaqMaster.Core.DnnSf
{
    public class DnnUrl
    {
        #region URL Parts

        /// <summary>
        /// http:// or https://
        /// </summary>
        public string Scheme { get; set; }

        /// <summary>
        /// The portal alias in this URL
        /// </summary>
        public string PortalAlias { get; private set; }

        // to which portal does this url refer
        public int PortalId { get; private set; }

        /// <summary>
        /// Root application path. Normally this is "/" except when the website runs in an 
        /// IIS virtual directory or the portal is a child portal.
        /// </summary>
        public string AppPath { get; private set; }

        /// <summary>
        /// The locale appears at the beginning of the url, for example example.com/en-us/blah...
        /// </summary>
        public string Locale { get; set; }

        /// <summary>
        /// This is the actual path after removing the AppPath, Locale and extension
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The extension is usually .aspx. But it could be as well a request to an .ashx or it could be nothing for extensionless URLs.
        /// And with a URL rewriter, it really can be anything
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Tells either this URL points to an external location
        /// </summary>
        public bool IsExternal { get; private set; }

        /// <summary>
        /// The parsed and sanitzied query string
        /// </summary>
        public NameValueCollection QueryString { get; set; }

        #endregion

        public DnnUrl()
        {
            AppPath = "/";
            Path = "";
            Extension = ""; 
            QueryString = new NameValueCollection();
        }

        public DnnUrl(string appPath)
        {
            QueryString = new NameValueCollection();
            MakeRelative(appPath);
        }

        public DnnUrl(int portalId, string alias, char pathSeparator = '/')
        {
            QueryString = new NameValueCollection();
            MakeAbsolute(portalId, alias, pathSeparator = '/');
        }

        public DnnUrl(int portalId, string alias, Uri url, char pathSeparator = '/')
        {
            QueryString = new NameValueCollection();
            MakeAbsolute(portalId, alias, pathSeparator = '/');
            LoadFromUrl(url, pathSeparator);
        }

        public DnnUrl(Uri url, char pathSeparator = '/')
        {
            LoadFromUrl(url, pathSeparator);
        }

        public void LoadFromUrl(Uri url, char pathSeparator = '/')
        {
            if (url.IsAbsoluteUri) {
                Scheme = url.Scheme;
                var alias = PortalControllerEx.GetCurrentPortalAlias(url);
                if (alias == null) {
                    // this is external URL
                    IsExternal = true;
                    MakeAbsolute(-1, url.Host);
                } else {
                    MakeAbsolute(alias.PortalID, alias.HTTPAlias);
                }
            } else {
                url = new Uri(new Uri("http://example.com"), url);
            }

            // extract relative URL, eliminating app path, locale prefix and extension
            Path = url.AbsolutePath.Substring(Math.Min(AppPath.Length, url.AbsolutePath.Length));
          
            if (Path.Length > 1) {
                var locales = LocaleController.Instance.GetLocales(PortalId);
                foreach (var locale in locales.Keys) {

                    //check that path starts with locale, and that the next char after that is either the separator or end of url
                    if (locale != null && Path.StartsWith(locale, StringComparison.OrdinalIgnoreCase) &&
                           (Path.Length == locale.Length || Path[locale.Length] == pathSeparator)) {

                        Locale = locale;

                        // also eliminate locale from path
                        Path = Path.Substring(locale.Length);

                        // change path separator
                        if (Path.Length > 0)
                            Path = pathSeparator + Path.Substring(1);

                        break;
                    }
                }
            }

           

            // extract extension
            Extension = "";
            if (!string.IsNullOrEmpty(Path)) {
                Extension = VirtualPathUtility.GetExtension(Path);
                if (Extension.Length > 0)
                    Path = Path.Substring(0, Path.Length - Extension.Length);
            }

            // parse query string
            QueryString = new NameValueCollection();
            var queryStr = url.Query;
            if (!string.IsNullOrEmpty(queryStr)) {

                // fix URLs that have double ? ? - because DNN sometimes blindly appends ?popup=true without concerns for actual URL format
                if (queryStr.LastIndexOf('?') > 0)
                    queryStr = queryStr[0] + queryStr.Substring(1).Replace('?', '&');

                // could name be null?
                var parsedQuery = HttpUtility.ParseQueryString(queryStr);
                foreach (var name in parsedQuery.AllKeys)
                    if (!string.IsNullOrEmpty(name))
                        QueryString[name] = parsedQuery[name];
            }
        }

        /// <summary>
        /// Absolute URLS can only be built if we have a Portal Alias.
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="pathSeparator"></param>
        public void MakeAbsolute(int portalId, string alias, char pathSeparator = '/')
        {
            if (alias == null)
                throw new ArgumentNullException("Invalid portal alias");

            PortalId = portalId;
            PortalAlias = alias;

            // extract app path from alias. Note that first separator after domain name is always /
            AppPath = "/";
            var iPath = PortalAlias.IndexOf('/');
            if (iPath != -1 && iPath != PortalAlias.Length - 1)
                AppPath = PortalAlias.Substring(iPath).TrimEnd('/') + pathSeparator;
        }

        /// <summary>
        /// Provide an App Path to build accurate relative URLs.
        /// </summary>
        /// <param name="appPath"></param>
        public void MakeRelative(string appPath)
        {
            AppPath = appPath;
        }

        public string Build(UriKind type, bool extensionless = false, bool lowercase = false, char separator = '/', Func<string, string> tokenize = null)
        {
            if (type == UriKind.Absolute && PortalAlias == null)
                throw new ArgumentNullException("Can't build absolute URL without portal alias");

            // don't change case for external URLs
            if (IsExternal)
                lowercase = false;

            var sb = new StringBuilder();

            // do we build an absolute URL or a relative URL?
            string baseUrl = type == UriKind.Absolute || (type == UriKind.RelativeOrAbsolute && PortalAlias != null) ?
                (Scheme ?? "http") + "://" + PortalAlias + separator : AppPath;

            sb.Append(lowercase ? baseUrl.ToLower() : baseUrl);

            //if (type == UriKind.Absolute || (type == UriKind.RelativeOrAbsolute && PortalAlias != null)) {
            //    baseUrl = Scheme + PortalAlias;
            //    //sb.Append(Scheme);

            //    //// append alias
            //    //sb.AppendFormat("{0}{1}", lowercase ? PortalAlias.ToLower() : PortalAlias, separator);
            //} else {
            //    // append app path
            //    sb.AppendFormat("{0}{1}", lowercase ? AppPath.ToLower() : AppPath, separator);
            //}

            // append locale
            if (!IsExternal) {
                if (!string.IsNullOrEmpty(Locale)) {
                    sb.AppendFormat("{0}{1}", lowercase ? Locale.ToLower() : Locale, separator);
                } else if (QueryString["language"] != null) {
                    sb.AppendFormat("{0}{1}", lowercase ? QueryString["language"].ToLower() : QueryString["language"], separator);
                }
            }

            // append path and query
            sb.Append(lowercase ? Path.ToLower() : Path);

            if (!extensionless)
                sb.Append(lowercase ? Extension.ToLower() : Extension);
            

            var skipParams = new string[] { "tabid", "language" };
            sb.Append(UriUtil.BuildQueryString(QueryString, true, lowercase, tokenize, skipParams));

            return sb.ToString();
        }

        public override string ToString()
        {
            return Build(UriKind.RelativeOrAbsolute);
        }
    }
}
