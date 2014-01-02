using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnnSharp.FaqMaster.Core.DnnSf.Orm.ScaleConfig
{
    public class PropertyAttribute : Attribute
    {
        public object Default { get; set; }

        public PropertyAttribute()
        {
        }

    }
}
