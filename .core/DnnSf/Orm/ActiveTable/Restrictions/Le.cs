using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnnSharp.FaqMaster.Core.DnnSf.Orm.ActiveTable
{
    public class Le: IRestriction
    {
        public string Name { get; private set; }
        public object Val { get; private set; }

        public Le(string name, object val)
        {
            Name = name;
            Val = val;
        }

        public string ToSql()
        {
            return string.Format("{0}<={1}", Name, Table.EncodeSql(Val));
        }
    }
}
