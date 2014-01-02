using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DnnSharp.FaqMaster.Core.DnnSf.Orm.ActiveTable
{
   
    internal class Field
    {
        public PropertyInfo Property { get; private set; }
        public FieldAttribute Attr { get; private set; }

        public Field(PropertyInfo property)
        {
            Property = property;
            Attr = Property.GetCustomAttributes(false).FirstOrDefault(x => x is FieldAttribute) as FieldAttribute;
        }

        public string GetQueryPart()
        {
            var name = string.IsNullOrEmpty(Attr.Name) ? Property.Name : Attr.Name;

            if (!string.IsNullOrEmpty(Attr.Formula))
                return string.Format("({0}) as {1}", TokenUtil.ReplaceDbOwnerAndPrefix(Attr.Formula), name);

            return name;
        }

        public bool CanInsert
        {
            get { return string.IsNullOrEmpty(Attr.Formula); }
        }

        public bool CanUpdate
        {
            get { return string.IsNullOrEmpty(Attr.Formula); }
        }
    }
}
