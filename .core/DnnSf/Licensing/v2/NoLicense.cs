using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnnSharp.FaqMaster.Core.DnnSf.Licensing.v2
{
    public class NoLicense : LicenseBase
    {
        public override LicenseStatus Status
        {
            get
            {
                return new LicenseStatus() {
                    Type = LicenseStatus.eType.Error,
                    Code = LicenseStatus.eCode.Invalid,
                    Message = "No license file present"
                };
            }
        }
    }
}
