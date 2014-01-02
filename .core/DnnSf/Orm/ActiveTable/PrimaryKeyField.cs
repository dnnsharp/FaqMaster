using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DnnSharp.FaqMaster.Core.DnnSf.Orm.ActiveTable
{
    internal class PrimaryKeyField
    {
        public PropertyInfo Property { get; private set; }
        public PrimaryKeyAttribute Attr { get; private set; }

        public PrimaryKeyField(PropertyInfo property)
        {
            Property = property;
            Attr = Property.GetCustomAttributes(false).FirstOrDefault(x => x is PrimaryKeyAttribute) as PrimaryKeyAttribute;
        }

        public string GetQueryPart()
        {
            return string.IsNullOrEmpty(Attr.Name) ? Property.Name : Attr.Name;
        }
    }
}
