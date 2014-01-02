using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnnSharp.FaqMaster.Core.DnnSf.Logging
{
    public enum eLogLevel
    {
        // No logging
        Off = 0,

        // errors only
        Error = 1,
        Errors = 1,

        // errors and some info
        Info = 2,

        // include debug information as well
        Debug = 3
    }

}
