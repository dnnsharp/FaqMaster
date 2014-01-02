using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnnSharp.FaqMaster.Core.DnnSf
{
    public static class ExceptionUtils
    {
        public static string FlattenException(this Exception exception)
        {
            if (exception == null) {
                return "NULL???";
            }

            var sb = new StringBuilder();

            //var ic = 0;
            while (exception != null) {
                sb.AppendLine(exception.GetType().AssemblyQualifiedName);
                sb.AppendLine(exception.Message ?? "");
                sb.AppendLine(exception.StackTrace ?? "");

                exception = exception.InnerException;
                //ic++;
                //if (ic > 10) {
                //    sb.AppendLine(".... and more");
                //    break;
                //}
            }

            return sb.ToString();
        }
    }
}
