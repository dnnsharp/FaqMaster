using DnnSharp.FaqMaster.Core.DnnSf.Logging.Target;
using DotNetNuke.Entities.Portals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnnSharp.FaqMaster.Core.DnnSf.Logging
{
    public static class LoggerUtil
    {
        public static string FlattenException(Exception exception)
        {
            var sb = new StringBuilder();

            while (exception != null) {
                sb.AppendLine(exception.Message);
                sb.AppendLine(exception.StackTrace);

                exception = exception.InnerException;
            }

            return sb.ToString();
        }
    }
}
