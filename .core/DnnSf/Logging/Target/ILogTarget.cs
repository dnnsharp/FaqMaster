using DotNetNuke.Entities.Portals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnnSharp.FaqMaster.Core.DnnSf.Logging.Target
{
    public interface ILogTarget<T>
        where T : class
    {
        void Log(eLogLevel level, T data, string message);
        void Log(eLogLevel level, T data, Exception ex);
    }
}
