using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace DnnSharp.FaqMaster.Core.DnnSf
{
    public class WebConfigUtil
    {
        public enum eCreateMethod
        {
            DontCreate,
            Append,
            Prepend
        }

        public enum eProviderType
        {
            friendlyUrl
        }

        XmlDocument _doc;

        public WebConfigUtil()
        {
            _doc = new XmlDocument();
            _doc.Load(App.RootPath + "/web.config");
        }

        public void UpdateModule(string name, string type, eCreateMethod create)
        {
            // first, do it for system.webServer
            var section = _doc.SelectSingleNode("//system.webServer/modules") as XmlElement;
            UpdateModule(section, name, type, create);

            // now, do system.web (IE6), but only if the section exists, otherwise there's a good reason why it's not there
            section = _doc.SelectSingleNode("//system.web/httpModules") as XmlElement;
            if (section != null)
                UpdateModule(section, name, type, create);
        }

        void UpdateModule(XmlElement section, string name, string type, eCreateMethod create)
        {
            // first, do it for webServer
            var mod = section.SelectSingleNode("add[@name='" + name + "']") as XmlElement;
            if (mod != null) {
                mod.SetAttribute("type", type);
            } else if (create != eCreateMethod.DontCreate) {
                mod = _doc.CreateElement("add");
                mod.SetAttribute("name", name);
                mod.SetAttribute("type", type);
                if (create == eCreateMethod.Append) {
                    section.AppendChild(mod);
                } else {
                    section.PrependChild(mod);
                }
            }
        }


        public void AddProvider(eProviderType providerType, string name, string type, NameValueCollection attrs = null)
        {
            var node = _doc.SelectSingleNode("//dotnetnuke/" + providerType.ToString() + "/providers") as XmlElement;
            if (node == null)
                throw new ArgumentException("Invalid provider " + providerType.ToString());

            XmlElement provider = node.SelectSingleNode("add[@name='" + name + "']") as XmlElement;
            if (provider == null) {
                provider = _doc.CreateElement("add");
                node.AppendChild(provider);
            }

            provider.SetAttribute("name", name);
            provider.SetAttribute("type", type);
            if (attrs != null) {
                foreach (string attrName in attrs)
                    provider.SetAttribute(attrName, attrs[attrName]);
            }

        }

        public void SetDefaultProvider(eProviderType providerType, string defaultProvider)
        {
            var node = _doc.SelectSingleNode("//dotnetnuke/" + providerType.ToString()) as XmlElement;
            if (node == null)
                throw new ArgumentException("Invalid provider " + providerType.ToString());

            node.SetAttribute("defaultProvider", defaultProvider.ToString());
        }

        public void Comment(string xpath)
        {
            foreach (XmlNode node in _doc.SelectNodes(xpath))
                node.ParentNode.ReplaceChild(_doc.CreateComment(node.OuterXml), node);
        }

        public void Uncomment(string xpath)
        {
            foreach (XmlComment commentNode in _doc.SelectNodes("//comment()")) {

                try {
                    XmlNode newNode = _doc.CreateElement("wrapper");
                    newNode.InnerXml = commentNode.Value;

                    if (newNode.SelectSingleNode(xpath) != null)
                        commentNode.ParentNode.ReplaceChild(newNode.ChildNodes[0], commentNode);

                } catch { continue; }

                
            }
        }


        public void Save()
        {
            _doc.Save(App.RootPath + "/web.config");
        }
    }
}
