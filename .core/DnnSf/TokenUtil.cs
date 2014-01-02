using System;
using System.Text;
using System.Web;

namespace DnnSharp.FaqMaster.Core.DnnSf
{
    public class TokenUtil
    {

        public enum eContext
        {
            None,
            Sql,
            Html,
            Url,
            Script
        }
        public static string ReplaceDbOwnerAndPrefix(string sql)
        {
            return sql.Replace("{databaseOwner}", DotNetNuke.Common.Utilities.Config.GetDataBaseOwner())
                      .Replace("{objectQualifier}", DotNetNuke.Common.Utilities.Config.GetObjectQualifer());
        }

        public static string ReplaceCommonPaths(string content)
        {
            if (string.IsNullOrEmpty(content))
                return content;

            return Tokenize(content, (token) => {
                switch (token.ToLower()) {
                    case "apppath":
                        return App.BasePath;
                    case "appurl":
                        return App.BaseUrl;
                    case "dnnpath":
                        return App.RootPath;
                    case "dnnurl":
                        return App.RootUrl;
                    default:
                        return token;
                }
            }, true, "{}");
        }

        public static string Tokenize(string content, Func<string, string> fnReplace, bool performDnnTokenReplacementToo = false, string delim = "[]")
        {
            if (string.IsNullOrEmpty(content))
                return content;

            var sb = new StringBuilder();
            int iLastToken = 0;
            int iBracket = 0;

            while ((iBracket = content.IndexOf(delim[0], iBracket)) != -1) {

                int iEndBracket = content.IndexOf(delim[1], iBracket + 1);
                if (iEndBracket == -1)
                    break; // no match, no token

                var strCandidate = content.Substring(iBracket, iEndBracket - iBracket + 1);

                // add text in between
                if (iBracket != iLastToken)
                    sb.Append(content.Substring(iLastToken, iBracket - iLastToken));

                sb.Append(fnReplace(strCandidate.Trim(delim.ToCharArray())));
                iBracket = iLastToken = iEndBracket + 1;
            }

            // append rest of the text
            if (iLastToken != content.Length)
                sb.Append(content.Substring(iLastToken));

            if (performDnnTokenReplacementToo)
                return Tokenize(sb.ToString());

            return sb.ToString();
        }

        public static string Tokenize(string strContent)
        {
            return Tokenize(strContent, null, null, false, true);
        }

        public static bool TokenizeAsBool(string strContent, DotNetNuke.Entities.Modules.ModuleInfo modInfo = null,
            DotNetNuke.Entities.Users.UserInfo user = null, bool forceDebug = false, bool bRevertToDnn = true)
        {
            var result = Tokenize(strContent, modInfo, user, forceDebug, bRevertToDnn);

            bool isValid;
            if (bool.TryParse(result, out isValid))
                return isValid;

            // also parse it as a number
            return result == "1";
        }

        public static string Tokenize(string strContent, DotNetNuke.Entities.Modules.ModuleInfo modInfo, DotNetNuke.Entities.Users.UserInfo user, bool forceDebug, bool bRevertToDnn)
        {
            string cacheKey_Installed = "avt.MyTokens2.Installed";
            string cacheKey_MethodReplace = "avt.MyTokens2.MethodReplace";

            string bMyTokensInstalled = "no";
            System.Reflection.MethodInfo methodReplace = null;

            bool bDebug = forceDebug;
            if (!bDebug) {
                try { bDebug = DotNetNuke.Common.Globals.IsEditMode(); } catch { }
            }

            lock (typeof(DotNetNuke.Services.Tokens.TokenReplace)) {
                // first, determine if MyTokens is installed
                if (HttpRuntime.Cache.Get(cacheKey_Installed) == null) {

                    // check again, maybe current thread was locked by another which did all the work
                    if (HttpRuntime.Cache.Get(cacheKey_Installed) == null) {

                        // it's not in cache, let's determine if it's installed
                        try {
                            Type myTokensRepl = DotNetNuke.Framework.Reflection.CreateType("avt.MyTokens.MyTokensReplacer", true);
                            if (myTokensRepl == null)
                                throw new Exception(); // handled in catch

                            bMyTokensInstalled = "yes";

                            // we now know MyTokens is installed, get ReplaceTokensAll methods
                            methodReplace = myTokensRepl.GetMethod(
                                "ReplaceTokensAll",
                                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
                                null,
                                System.Reflection.CallingConventions.Any,
                                new Type[] { 
                                    typeof(string), 
                                    typeof(DotNetNuke.Entities.Users.UserInfo), 
                                    typeof(bool),
                                    typeof(DotNetNuke.Entities.Modules.ModuleInfo)
                                },
                                null
                            );

                            if (methodReplace == null) {
                                // this shouldn't really happen, we know MyTokens is installed
                                throw new Exception();
                            }

                        } catch {
                            bMyTokensInstalled = "no";
                        }

                        // cache values so next time the funciton is called the reflection logic is skipped
                        HttpRuntime.Cache.Insert(cacheKey_Installed, bMyTokensInstalled);
                        if (bMyTokensInstalled == "yes") {
                            HttpRuntime.Cache.Insert(cacheKey_MethodReplace, methodReplace);
                        }
                    }
                }
            }

            bMyTokensInstalled = HttpRuntime.Cache.Get(cacheKey_Installed).ToString();
            if (bMyTokensInstalled == "yes") {
                methodReplace = (System.Reflection.MethodInfo)HttpRuntime.Cache.Get(cacheKey_MethodReplace);
                if (methodReplace == null) {
                    HttpRuntime.Cache.Remove(cacheKey_Installed);
                    return Tokenize(strContent, modInfo, user, forceDebug, bRevertToDnn);
                }
            } else {
                // if it's not installed return string or tokenize with DNN replacer
                if (!bRevertToDnn) {
                    return strContent;
                } else {
                    DotNetNuke.Services.Tokens.TokenReplace dnnTknRepl = new DotNetNuke.Services.Tokens.TokenReplace();
                    dnnTknRepl.AccessingUser = DotNetNuke.Entities.Users.UserController.GetCurrentUserInfo();
                    dnnTknRepl.User = user ?? DotNetNuke.Entities.Users.UserController.GetCurrentUserInfo();
                    dnnTknRepl.DebugMessages = bDebug;
                    if (modInfo != null)
                        dnnTknRepl.ModuleInfo = modInfo;

                    // MyTokens is not installed, execution ends here
                    return dnnTknRepl.ReplaceEnvironmentTokens(strContent);
                }
            }

            // we have MyTokens installed, proceed to token replacement
            return (string)methodReplace.Invoke(
                null,
                new object[] {
                    strContent,
                    user ?? DotNetNuke.Entities.Users.UserController.GetCurrentUserInfo(),
                    bDebug,
                    modInfo
                }
            );

        }

    }

    

}
