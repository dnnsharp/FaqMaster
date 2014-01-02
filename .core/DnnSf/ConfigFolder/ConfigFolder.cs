using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Web;
using System.IO;

namespace DnnSharp.FaqMaster.Core.DnnSf.ConfigFolder
{
    public class ConfigFolder<T>
        where T : new()
    {
        string _MasterCacheKey;
        string _FolderPath;

        public ConfigFolder(string folderPath, string masterCacheKey)
        {
            if (folderPath != null)
                _FolderPath = folderPath.TrimEnd('\\', '/');

            _MasterCacheKey = masterCacheKey;
        }

        public string CacheKey { get { return typeof(T).ToString() + ".Config"; } }

        static object _lockItems = new object();

        public Dictionary<string, T> GetItems(string xpathItem = null, string xpathItemId = "Id")
        {
            lock (_lockItems) {

                if (HttpRuntime.Cache.Get(CacheKey) != null)
                    return HttpRuntime.Cache.Get(CacheKey) as Dictionary<string, T>;

                // build list of file depenendecies so we refresh cache when any of them changes
                List<string> depFiles = new List<string>();
                depFiles.Add(GetDefaultConfig(_FolderPath));
                depFiles.Add(_FolderPath);

                // load defaults first
                Dictionary<string, T> itemsById = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
                itemsById = ReadConfig(itemsById, depFiles[0], xpathItem, xpathItemId);

                // now load overrides
                foreach (string configFile in Directory.GetFiles(_FolderPath)) {
                    if (Path.GetFileName(configFile)[0] == '.' || (Path.GetExtension(configFile) != ".xml" && Path.GetExtension(configFile) != ".config"))
                        continue;

                    itemsById = ReadConfig(itemsById, GetConfigFile(configFile), xpathItem, xpathItemId);
                }

                // add it to cache
                HttpRuntime.Cache.Add(
                    CacheKey,
                    itemsById,
                    new System.Web.Caching.CacheDependency(depFiles.ToArray(), new string[] { _MasterCacheKey }),
                    DateTime.Now.AddDays(1),
                    System.Web.Caching.Cache.NoSlidingExpiration,
                    System.Web.Caching.CacheItemPriority.Normal,
                    null
                );

                return itemsById;
            }
        }

        private Dictionary<string, T> ReadConfig(Dictionary<string, T> itemsById, string confFile, string xpathItem, string xpathItemId)
        {
            //try {
            //    using (var fs = File.OpenRead(confFile)) {
            //        System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(List<T>));
            //        foreach (var item in xmlSerializer.Deserialize(fs) as List<T>) {

            //        }
            //    }
            //} catch (Exception e) {
            //    throw new Exception("Error parsing configuration file " + confFile, e);
            //}

            //if (Path.GetExtension(confFile) == ".xml") {
            //    // rename to .config for enhanced security
            //    File.Move(confFile, Path.ChangeExtension(confFile, ".config"));
            //    confFile = Path.ChangeExtension(confFile, ".config");
            //}

            XmlDocument xmlConfFile = new XmlDocument();
            xmlConfFile.Load(confFile);

            var itemDefinitions = string.IsNullOrEmpty(xpathItem)
                ? xmlConfFile.DocumentElement.ChildNodes
                : xmlConfFile.DocumentElement.SelectNodes(xpathItem);

            foreach (XmlNode xmlDef in itemDefinitions) {

                if (xmlDef.NodeType != XmlNodeType.Element)
                    continue;

                try {
                    string id = xmlDef.SelectSingleNode(xpathItemId).InnerText.Trim();

                    using (var fs = new MemoryStream(UTF8Encoding.Default.GetBytes(xmlDef.OuterXml ?? ""))) {
                        System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                        itemsById[id] = (T)xmlSerializer.Deserialize(fs);

                        if (itemsById[id] is INotifyDone)
                            (itemsById[id] as INotifyDone).Done();
                    }
                } catch (Exception e) {
                    throw new Exception("Error parsing configuration file " + confFile, e);
                }
            }

            return itemsById;
        }

        /// <summary>
        /// returns .defaults.config, renaming Defaults.xml to .defaults.config if it exists
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        string GetDefaultConfig(string folder)
        {
            foreach (var variant in new string[] { ".defaults.config", "Defaults.config", ".defaults.xml", "Defaults.xml" }) {
                var goodConfig = Path.Combine(folder, variant);
                if (File.Exists(goodConfig))
                    return goodConfig;
            }

            return null;
        }

        /// <summary>
        /// This migrates .xml files to .config files to ensure enhanced security
        /// </summary>
        /// <param name="configFile"></param>
        /// <returns></returns>
        string GetConfigFile(string configFile)
        {
            var goodConfig = Path.ChangeExtension(configFile, ".config");
            if (File.Exists(goodConfig))
                return goodConfig;

            return configFile;
        }

    }
}
