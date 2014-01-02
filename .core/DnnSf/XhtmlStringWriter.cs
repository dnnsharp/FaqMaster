using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnnSharp.FaqMaster.Core.DnnSf
{
    public class XhtmlStringWriter : System.IO.StringWriter
    {
        public XhtmlStringWriter()
        {
        }

        public XhtmlStringWriter(StringBuilder sb)
            :base(sb)
        {
        }



        public override void Write(string value)
        {
            bool isShortNotation = false;
            switch (value) {
                case "></area>":
                    isShortNotation = true;
                    break;
                case "></base>":
                    isShortNotation = true;
                    break;
                case "></basefont>":
                    isShortNotation = true;
                    break;
                case "></bgsound>":
                    isShortNotation = true;
                    break;
                case "></br>":
                    isShortNotation = true;
                    break;
                case "></col>":
                    isShortNotation = true;
                    break;
                case "></frame>":
                    isShortNotation = true;
                    break;
                case "></hr>":
                    isShortNotation = true;
                    break;
                case "></img>":
                    isShortNotation = true;
                    break;
                case "></input>":
                    isShortNotation = true;
                    break;
                case "></isindex>":
                    isShortNotation = true;
                    break;
                case "></keygen>":
                    isShortNotation = true;
                    break;
                case "></link>":
                    isShortNotation = true;
                    break;
                case "></meta>":
                    isShortNotation = true;
                    break;
                case "></param>":
                    isShortNotation = true;
                    break;
            }

            if (isShortNotation) {
                base.Write(" />");
            } else {
                base.Write(value);
            }
        }
    }
}
