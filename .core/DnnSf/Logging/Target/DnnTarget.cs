using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Log.EventLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnnSharp.FaqMaster.Core.DnnSf.Logging.Target
{
    public class DnnTarget<T> : ILogTarget<T>
        where T : class
    {
        public void Log(eLogLevel level, T data, string message)
        {
            // we only log errors with this target so we don't flood the event log
            if (level != eLogLevel.Error && level != eLogLevel.Errors)
                return;

            Log(level, data, new Exception(message));
        }

        public void Log(eLogLevel level, T data, Exception ex)
        {
            // we only log errors with this target so we don't flood the event log
            if (level != eLogLevel.Error && level != eLogLevel.Errors)
                return;

            Exceptions.LogException(ex);
        }

    }
}
