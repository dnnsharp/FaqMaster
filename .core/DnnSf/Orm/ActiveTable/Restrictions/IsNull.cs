using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnnSharp.FaqMaster.Core.DnnSf.Orm.ActiveTable
{
    public class IsNull : IRestriction
    {
        public string Name { get; private set; }

        public IsNull(string name)
        {
            Name = name;
        }

        public string ToSql()
        {
            return string.Format("{0} IS NULL", Name);
        }
    }
}
