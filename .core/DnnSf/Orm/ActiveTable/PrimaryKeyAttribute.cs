using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnnSharp.FaqMaster.Core.DnnSf.Orm.ActiveTable
{
    internal class PrimaryKeyAttribute : Attribute
    {
        public string Name { get; set; }
        public bool IsIdentity { get; set; }

        public PrimaryKeyAttribute(bool isIdentity = true)
        {
            IsIdentity = isIdentity;
        }
    }
}
