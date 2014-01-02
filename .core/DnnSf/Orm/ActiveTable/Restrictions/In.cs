using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnnSharp.FaqMaster.Core.DnnSf.Orm.ActiveTable
{
    public class In : IRestriction
    {
        public string Name { get; private set; }
        public IEnumerable List { get; private set; }

        public In(string name, IEnumerable list)
        {
            Name = name;
            List = list;
        }

        public string ToSql()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("{0} IN (", Name);
            foreach (var o in (IEnumerable)List)
                sb.AppendFormat("{0},", Table.EncodeSql(o));
            if (sb[sb.Length - 1] == ',')
                sb.Remove(sb.Length - 1, 1);
            sb.Append(")");

            return sb.ToString();
        }
    }
}
