using System;
using System.Web;

namespace DnnSharp.FaqMaster
{
    public partial class FAQMasterApi : DotNetNuke.Framework.CDefault
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            Response.Cache.SetValidUntilExpires(false);
            Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            if (Request.Params["cmd"] == null)
                return;

            switch (Request.Params["cmd"].ToLower()) {
                case "getfaq":
                    break;
            }
        }
    }
}