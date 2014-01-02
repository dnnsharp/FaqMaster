using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace DnnSharp.FaqMaster.Core.DnnSf.Html
{
    public static class CssUtil
    {
        public static void AppendStyles(Page page, string css)
        {
            HtmlGenericControl cssStyles = new HtmlGenericControl("style");
            cssStyles.Attributes["type"] = "text/css";
            cssStyles.InnerText = css;

            page.Header.Controls.Add(cssStyles);
        }
    }
}
