using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace DnnSharp.FaqMaster.Core.DnnSf
{
    public static class XmlUtil
    {
        /// <summary>
        /// Some methods accept an XmlWriter as parameter for serialization. 
        /// Bug getting it as a string implies some additional coding. 
        /// This function takes care of initializing an XmlWriter and passing it to your delegate function, returning the XML as string
        /// </summary>
        /// <param name="fn"></param>
        /// <returns></returns>
        public static string Serialize(Action<XmlWriter> fn)
        {
            StringBuilder strXML = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            XmlWriter writer = XmlWriter.Create(new StringWriterWithEncoding(strXML, Encoding.UTF8), settings);

            fn(writer);
            writer.Close();

            return strXML.ToString();
        }

        public static XmlElement UpdateChildElement(this XmlNode elem, string name, string innerText = null)
        {
            var child = elem[name];
            if (child == null) {
                var doc = (elem is XmlDocument ? elem : elem.OwnerDocument) as XmlDocument;
                child = doc.CreateElement(name);
                elem.AppendChild(child);
            }

            child.InnerText = innerText;
            return child;
        }
    }
}
