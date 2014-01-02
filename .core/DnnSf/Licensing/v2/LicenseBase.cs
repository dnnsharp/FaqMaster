using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnnSharp.FaqMaster.Core.DnnSf.Licensing.v2
{
    public abstract class LicenseBase
    {
        public LicenseInfo License { get; set; }

        public string Type { get { return GetType().Name; } }

        public abstract LicenseStatus Status { get; }

    }
}
