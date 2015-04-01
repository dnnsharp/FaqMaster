using DnnSharp.Common.Dnn;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Search;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;

namespace DnnSharp.FaqMaster.Core
{
    public class FaqMasterSettings
    {
        public int ModuleId = -1;
        public string Template = "default";
        public bool UseAjax = false;
        public bool UseOwnjQuery = true;
        public string ShowEffect = "none";
        public string HideEffect = "none";


        public FaqMasterSettings Load(int moduleId)
        {
            ModuleController modCtrl = new ModuleController();
            Hashtable modSettings = modCtrl.GetModuleSettings(moduleId);

            ModuleId = moduleId;
            
            if (modSettings.ContainsKey("Template")) {
                Template = modSettings["Template"].ToString();
            }

            // load AJAX switch

            try {
                UseAjax = Convert.ToBoolean(modSettings["UseAjax"]);
            } catch { UseAjax = false; }


            // load jQuery switch
            try {
                if (modSettings["UseOwnjQuery"] == null || string.IsNullOrEmpty(modSettings["UseOwnjQuery"].ToString())) {
                    throw new Exception();
                }
                UseOwnjQuery = Convert.ToBoolean(modSettings["UseOwnjQuery"]);
            } catch { UseOwnjQuery = true; }

            // load show effect
            if (modSettings.ContainsKey("ShowEffect")) {
                ShowEffect = modSettings["ShowEffect"].ToString();
            }

            // load hide effect
            if (modSettings.ContainsKey("HideEffect")) {
                HideEffect = modSettings["HideEffect"].ToString();
            }

            return this;
        }

        public string TemplatePath {
            get {
                return string.Format("{0}\\templates\\{1}", App.Info.BasePath, Template);
            }
        }

        public string TemplatePathMain {
            get { return TemplatePath + "\\main.xsl"; }
        }
    }

    public class FaqInfo
    {
        public FaqInfo()
        {
            FaqId = -1;
        }

        public int FaqId { get; set; }

        public int ModuleId { get; set; }

        public string Question { get; set; }

        public string Answer { get; set; }

        public int ViewOrder { get; set; }
    }
    

    public class FaqMasterController : IPortable, ISearchable, IUpgradeable
    {
        public string _objectQualifier;
        public string _databaseOwner;

        public FaqMasterController()
        {
            // Read the configuration specific information for this provider
            DotNetNuke.Framework.Providers.ProviderConfiguration _providerConfiguration = DotNetNuke.Framework.Providers.ProviderConfiguration.GetProviderConfiguration("data");
            DotNetNuke.Framework.Providers.Provider objProvider = (DotNetNuke.Framework.Providers.Provider)_providerConfiguration.Providers[_providerConfiguration.DefaultProvider];

            // Read the attributes for this provider
            //Get Connection string from web.config
            string _connectionString = DotNetNuke.Common.Utilities.Config.GetConnectionString();

            if (_connectionString == "") {
                // Use connection string specified in provider
                _connectionString = objProvider.Attributes["connectionString"];
            }

            string _providerPath = objProvider.Attributes["providerPath"];

            _objectQualifier = objProvider.Attributes["objectQualifier"];
            if (_objectQualifier != "" & _objectQualifier.EndsWith("_") == false) {
                _objectQualifier += "_";
            }

            _databaseOwner = objProvider.Attributes["databaseOwner"];
            if (_databaseOwner != "" & _databaseOwner.EndsWith(".") == false) {
                _databaseOwner += ".";
            }
        }


        public XmlDocument MyTokens_GetDefinition()
        {
            // since token descriptors are static,
            // let MyTokens cache it for a reasonable amount of time;
            // if namespace info would be dynamic (for example, token definitions would change
            // based on current user roles). then set it to 0 (which is the default).
            // note this only affects retrieval of token definitions, caching values 
            // returned from tokens is treaded separately in MyTokens_Replace method
            //cacheTimeSeconds = 86400; // 1 day

            string xml = @"
                <mytokens><!-- namespace is Module Name -->
                    <receiveOnlyKnownTokens>true</receiveOnlyKnownTokens>
                    <cacheTimeSeconds>86400</cacheTimeSeconds><!-- instructs MyTokens to cache this token definition for specified amount of time; set to 0 to disable caching if token definitions are dynamic (for example, changes based on roles of current user or based on time events) -->
                    <docurl>http://docs.avatar-soft.ro</docurl>
                    <token>
                        <name>LatestFaq</name><!-- case insensitive -->
                        <desc>Returns latest faq.</desc>
                        <cacheTimeSeconds>20</cacheTimeSeconds>
                        <docurl>http://docs.avatar-soft.ro</docurl>
                        <param>
                            <name>ModuleId</name><!-- case insensitive -->
                            <desc>
                                Id of the module to extract latest faq for. 
                                Leave empty to retrieve latest FAQ from any FAQMaster module.
                            </desc>
                            <type>int</type>
                            <values></values><!-- only for enums, comma separated list -->
                            <default>-1</default>
                            <required>false</required>
                        </param>
                        <param>
                            <name>Item</name><!-- case insensitive -->
                            <desc>
                                Tells to retreive either the Question or the Answer.
                            </desc>
                            <type>enum</type>
                            <values>Question,Answer</values><!-- only for enums, comma separated list -->
                            <default>Question</default>
                            <required>false</required>
                        </param>
                        <example>
                            <codeSnippet>[FAQMaster:LatestFaq]</codeSnippet>
                            <desc>Returns question latest added FAQ from any FAQMaster module.</desc>
                        </example>
                        <example>
                            <codeSnippet>[FAQMaster:LatestFaq(Item=Answer)]</codeSnippet>
                            <desc>Returns answer of latest added FAQ from any FAQMaster module.</desc>
                        </example><example>
                            <codeSnippet>[FAQMaster:LatestFaq(ModuleId=123)]</codeSnippet>
                            <desc>Returns question latest added FAQ from FAQMaster module with id 123.</desc>
                        </example>
                        <example>
                            <codeSnippet>[FAQMaster:LatestFaq(ModuleId=123, Item=Answer)]</codeSnippet>
                            <desc>Returns answer of latest added FAQ from FAQMaster module with id 123.</desc>
                        </example>
                    </token>
                    <token>
                        <name>GetFaq</name><!-- case insensitive -->
                        <desc>Returns a faq by its Id.</desc>
                        <cacheTimeSeconds>60</cacheTimeSeconds>
                        <docurl>http://docs.avatar-soft.ro</docurl>
                        <param>
                            <name>id</name><!-- case insensitive -->
                            <desc>
                                Id of the Faq to retrieve.
                            </desc>
                            <type>int</type>
                            <values></values><!-- only for enums, comma separated list -->
                            <default>-1</default>
                            <required>true</required>
                        </param>
                        <param>
                            <name>item</name><!-- case insensitive -->
                            <desc>
                                Tells to retreive either the Question or the Answer.
                            </desc>
                            <type>enum</type>
                            <values>Question,Answer</values><!-- only for enums, comma separated list -->
                            <default>Question</default>
                            <required>false</required>
                        </param>
                    </token>
                </mytokens>
            ";

            XmlDocument xmlTknNs = new XmlDocument();
            xmlTknNs.LoadXml(xml);
            return xmlTknNs;

            //return new string[] { "Faqs", "LatestFaq" };
        }


        string DotNetNuke.Entities.Modules.IPortable.ExportModule(int ModuleID)
        {
            StringBuilder strXML = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            
            XmlWriter Writer = XmlWriter.Create(strXML, settings);
            Writer.WriteStartElement("faqs");

            foreach (FaqInfo faq in GetFaqs(ModuleID)) {
                Writer.WriteStartElement("faq");
                Writer.WriteElementString("question", faq.Question);
                Writer.WriteElementString("answer", faq.Answer);
                Writer.WriteElementString("order", faq.ViewOrder.ToString());
                Writer.WriteEndElement(); // faq
            }
            
            Writer.WriteEndElement(); // faqs
            Writer.Close();

            return strXML.ToString();
        }

        void DotNetNuke.Entities.Modules.IPortable.ImportModule(int ModuleID, string Content, string Version, int UserID)
        {
            XmlNode xmlFaqs = ModuleApi.GetImportNode(Content, "faqs");

            foreach (XmlElement xmlFaq in xmlFaqs.SelectNodes("/faqs/faq")) {
                int order = int.MaxValue;
                try {
                    order = Convert.ToInt32(xmlFaq["order"].InnerText);
                } catch { order = int.MaxValue; }

                try {
                    UpdateFaq(-1, ModuleID, xmlFaq["question"].InnerText, xmlFaq["answer"].InnerText, order);
                } catch { }
            }
        }


        SearchItemInfoCollection DotNetNuke.Entities.Modules.ISearchable.GetSearchItems(ModuleInfo ModInfo)
        {
            SearchItemInfoCollection searchItems = new SearchItemInfoCollection();

            foreach (FaqInfo faq in GetFaqs(ModInfo.ModuleID)) {
                searchItems.Add(new SearchItemInfo(ModInfo.ModuleTitle, faq.Question, -1, Null.NullDate, ModInfo.ModuleID, "faq-" + faq.FaqId, faq.Question + " " + faq.Answer, "faqid=" + faq.FaqId.ToString(), Null.NullInteger));
            }

            return searchItems;
        }



        public static string GetCacheKey(int moduleId)
        {
            return string.Format("FAQMaster.{0}", moduleId);
        }

        public static string GetCacheKeyDict(int moduleId)
        {
            return string.Format("FAQMaster.dict.{0}", moduleId);
        }

        public static string GetCacheKeyLatest(int moduleId)
        {
            return string.Format("FAQMaster.latest.{0}", moduleId);
        }

        public int UpdateFaq(int faqId, int moduleId, string question, string answer, int viewOrder)
        {
            // clear cache
            string cacheKey = GetCacheKey(moduleId);
            HttpRuntime.Cache.Remove(cacheKey);

            string cacheKeyDic = GetCacheKeyDict(moduleId);
            HttpRuntime.Cache.Remove(cacheKeyDic);

            string cacheKeyLatest = GetCacheKeyLatest(moduleId);
            HttpRuntime.Cache.Remove(cacheKeyLatest);

            string cacheKeyLatestAll = GetCacheKeyLatest(-1);
            HttpRuntime.Cache.Remove(cacheKeyLatestAll);

            return DataProvider.Instance().UpdateFaq(faqId, moduleId, question, answer, viewOrder);
        }

        public List<FaqInfo> GetFaqs(int moduleId)
        {
            string cacheKey = GetCacheKey(moduleId);
            if (HttpRuntime.Cache.Get(cacheKey) == null) {
                HttpRuntime.Cache.Insert(cacheKey, DotNetNuke.Common.Utilities.CBO.FillCollection<FaqInfo>(DataProvider.Instance().GetFaqs(moduleId)));
            }

            return (List<FaqInfo>) HttpRuntime.Cache.Get(cacheKey);
        }

        public FaqInfo GetLatestFaq(int moduleId)
        {
            string cacheKey = GetCacheKeyLatest(moduleId);
            if (HttpRuntime.Cache.Get(cacheKey) == null) {
                HttpRuntime.Cache.Insert(cacheKey, DotNetNuke.Common.Utilities.CBO.FillObject<FaqInfo>(DataProvider.Instance().GetLatestFaq(moduleId)));
            }

            return (FaqInfo) HttpRuntime.Cache.Get(cacheKey);
        }

        public void DeleteItem(int moduleId, int itemId)
        {
            // clear cache
            string cacheKey = GetCacheKey(moduleId);
            HttpRuntime.Cache.Remove(cacheKey);

            string cacheKeyDic = GetCacheKeyDict(moduleId);
            HttpRuntime.Cache.Remove(cacheKeyDic);

            string cacheKeyLatest = GetCacheKeyLatest(moduleId);
            HttpRuntime.Cache.Remove(cacheKeyLatest);

            string cacheKeyLatestAll = GetCacheKeyLatest(-1);
            HttpRuntime.Cache.Remove(cacheKeyLatestAll);

            DataProvider.Instance().DeleteItem(itemId);
        }

        public Dictionary<int, FaqInfo> GetFaqsDictionary(int moduleId)
        {
            string cacheKey = GetCacheKeyDict(moduleId);
            if (HttpRuntime.Cache.Get(cacheKey) == null) {
                Dictionary<int, FaqInfo> faqMap = new Dictionary<int, FaqInfo>();
                foreach (FaqInfo faq in GetFaqs(moduleId)) {
                    faqMap[faq.FaqId] = faq;
                }

                HttpRuntime.Cache.Insert(cacheKey, faqMap);
            }
            
            return (Dictionary<int, FaqInfo>)HttpRuntime.Cache.Get(cacheKey);
        }





        public string UpgradeModule(string Version)
        {
            // delete old FAQ folder
            var oldFaqFolder = App.RootPath + "\\DesktopModules\\avt.FAQMaster";
            if (Directory.Exists(oldFaqFolder)) {
                try {
                    Directory.Delete(oldFaqFolder);
                } catch {
                    // it's not that important to remove it
                }
            }

            return "done";
        }
    }

}