using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnnSharp.FaqMaster.Core.DnnSf.Orm.ActiveTable
{
    internal class HasManyAttribute : Attribute
    {
        public string ForeignKeyColumnName { get; set; }
        public string OrderBy { get; set; }

        public HasManyAttribute(string foreignKeyColumnName)
        {
            ForeignKeyColumnName = foreignKeyColumnName;
        }
    }
}
