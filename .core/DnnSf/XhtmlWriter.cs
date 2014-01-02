using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnnSharp.FaqMaster.Core.DnnSf
{
    public class XhtmlTextWriter : System.Xml.XmlTextWriter
    {
        private string tagName = string.Empty;
        private string elementNamespace = string.Empty;

        public XhtmlTextWriter(System.IO.TextWriter w)
            : base(w)
        {
        }

        public override void WriteEndElement()
        {
            bool isShortNotation = true;

            // Check if XHTML Namespace
            if (string.IsNullOrEmpty(this.elementNamespace) ||
                (this.elementNamespace.Contains("www.w3.org") &&
                    this.elementNamespace.Contains("xhtml"))) {
                switch (this.tagName) {
                    case "area":
                        isShortNotation = true;
                        break;
                    case "base":
                        isShortNotation = true;
                        break;
                    case "basefont":
                        isShortNotation = true;
                        break;
                    case "bgsound":
                        isShortNotation = true;
                        break;
                    case "br":
                        isShortNotation = true;
                        break;
                    case "col":
                        isShortNotation = true;
                        break;
                    case "frame":
                        isShortNotation = true;
                        break;
                    case "hr":
                        isShortNotation = true;
                        break;
                    case "img":
                        isShortNotation = true;
                        break;
                    case "input":
                        isShortNotation = true;
                        break;
                    case "isindex":
                        isShortNotation = true;
                        break;
                    case "keygen":
                        isShortNotation = true;
                        break;
                    case "link":
                        isShortNotation = true;
                        break;
                    case "meta":
                        isShortNotation = true;
                        break;
                    case "param":
                        isShortNotation = true;
                        break;
                    default:
                        isShortNotation = false;
                        break;
                }
            }

            if (isShortNotation) {
                base.WriteEndElement();
            } else {
                base.WriteFullEndElement();
            }
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            this.tagName = localName.ToLower();
            this.elementNamespace = ns;
            base.WriteStartElement(prefix, localName, ns);
        }

        public override void WriteStartDocument()
        {
            // Don't emit XML declaration
        }

        public override void WriteStartDocument(bool standalone)
        {
            // Don't emit XML declaration
        }
    }
}
