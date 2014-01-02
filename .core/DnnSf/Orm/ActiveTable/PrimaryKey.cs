using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DnnSharp.FaqMaster.Core.DnnSf.Orm.ActiveTable
{
    internal class PrimaryKey
    {
        /// <summary>
        /// Can be a single field primary key or a composite primary key
        /// </summary>
        public IList<PrimaryKeyField> KeyFields { get; private set; }

        /// <summary>
        /// indicates the primary key is an identity
        /// </summary>
        public bool IsIdentity { get; private set; }

        public PrimaryKey(Type type)
        {
            KeyFields = (from p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => Attribute.IsDefined(x, typeof(PrimaryKeyAttribute)))
                         select new PrimaryKeyField(p)).ToList();

            // TODO: should we allow entities without primary keys?
            if (KeyFields.Count() == 0)
                throw new ArgumentException("Could not find a primary key");

            if (KeyFields.Count() == 1) {
                var firstKey = KeyFields.First();
                IsIdentity = firstKey.Property.PropertyType == typeof(int) &&
                    (firstKey.Property.GetCustomAttributes(false).FirstOrDefault(x => x is PrimaryKeyAttribute) as PrimaryKeyAttribute).IsIdentity;
            }

        }
    }
}
