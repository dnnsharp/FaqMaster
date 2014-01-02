using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnnSharp.FaqMaster.Core.DnnSf.Orm.ActiveTable
{
    public class NotEq : IRestriction
    {
        public string Name { get; private set; }

        public object Val { get; private set; }

        public NotEq(string name, object val)
        {
            Name = name;
            Val = val;
        }

        public string ToSql()
        {
            // if val is a nullable type and is null, switch to IS NULL, otherwise = will not work
            if (ConvertUtils.IsNullable(Val) && Val == null)
                return string.Format("{0} IS NOT NULL", Name);
            return string.Format("{0}!={1}", Name, Table.EncodeSql(Val));
        }
    }
}
