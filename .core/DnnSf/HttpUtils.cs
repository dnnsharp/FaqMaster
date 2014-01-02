using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace DnnSharp.FaqMaster.Core.DnnSf
{
    public static class HttpUtils
    {
        public static void Redirect(HttpContext context, string url, bool permanent)
        {
            if (permanent) {
                context.Response.Clear();
                //context.Response.Headers.Clear();
                context.Response.ClearHeaders();
                context.Response.Status = "301 Moved Permanently";
                context.Response.StatusCode = 301;
                context.Response.AddHeader("Location", url);
                context.Response.End();
            } else {
                context.Response.Redirect(url, true);
            }
        }

        /// <summary>
        /// Issues a 410 Gone header response
        /// </summary>
        /// <param name="context"></param>
        public static void Gone(HttpContext context)
        {
            context.Response.Clear();
            context.Response.ClearHeaders();
            context.Response.Status = "410 Gone";
            context.Response.StatusCode = 410;
            context.Response.End();
        }

    }
}
