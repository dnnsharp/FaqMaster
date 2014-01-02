using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace DnnSharp.FaqMaster.Core.DnnSf
{
    public static class XslSimpleTransform
    {
        public static string Transform(string xml, string tplPath, XsltArgumentList args = null)
        {
            XslCompiledTransform transform = new XslCompiledTransform();
            XsltSettings settings = new XsltSettings();
            settings.EnableScript = true;
            transform.Load(tplPath, settings, new XmlUrlResolver());

            //var output = new System.IO.StringWriter();
            var sb = new StringBuilder();
            using (var sw = new XhtmlStringWriter(sb)) {
                using (var output = new XhtmlTextWriter(sw)) {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(xml);
                    transform.Transform(xmlDoc, args, output);

                    return sb.ToString();
                }
            }
            
        }
    }
}
