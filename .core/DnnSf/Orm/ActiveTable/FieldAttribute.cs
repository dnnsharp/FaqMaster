using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnnSharp.FaqMaster.Core.DnnSf.Orm.ActiveTable
{
    internal class FieldAttribute : Attribute
    {
        public string Name { get; set; }
        public string Formula { get; set; }

        public FieldAttribute()
        {
            
        }

    }
}
