using Microsoft.ApplicationBlocks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DnnSharp.FaqMaster.Core.DnnSf.Orm.ActiveTable
{
    internal class Table
    {
        // TODO: default to class nam
        public string ConnString { get; private set; }
        public string TableName { get; private set; }

        public PrimaryKey PrimaryKey { get; private set; }
        public IList<Field> Fields { get; private set; }
        public IList<Field> ManyRelations { get; private set; }

        public Table(Type type)
        {
            ConnString = DotNetNuke.Common.Utilities.Config.GetConnectionString();
            TableName = (type.GetCustomAttributes(false).FirstOrDefault(x => x is TableAttribute) as TableAttribute).Name;
            
            PrimaryKey = new PrimaryKey(type);

            Fields = (from p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => Attribute.IsDefined(x, typeof(FieldAttribute)))
                      select new Field(p)).ToList();

            ManyRelations = (from p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => Attribute.IsDefined(x, typeof(HasManyAttribute)))
                             select new Field(p)).ToList();

            if (!PrimaryKey.IsIdentity && ManyRelations.Count() > 0)
                throw new ArgumentException("HasMany only works with identity columns now");
        }

        public void ExecuteUpdate(object obj)
        {
            // compute SQL query
            string sql = SqlUpdate(obj);

            try {
                
                if (PrimaryKey.IsIdentity) {
                    PrimaryKey.KeyFields.First().Property.SetValue(obj, Convert.ToInt32(SqlHelper.ExecuteScalar(ConnString, CommandType.Text, sql, null)), null);
                } else {
                    SqlHelper.ExecuteNonQuery(ConnString, CommandType.Text, sql, null);
                }
            } catch (Exception ex) {
                throw new Exception("Failed to execute SQL " + sql, ex);
            }
        }


        public IList<T> GetAllByProperty<T>(string orderBy = null, int? top = null, params IRestriction[] criteria)
        {
            // TODO: validate propname?
            var sbSql = new StringBuilder();
            sbSql.Append("SELECT ");
            if (top.HasValue)
                sbSql.AppendFormat("TOP {0} ", top.Value);

            sbSql.Append(string.Join(",", PrimaryKey.KeyFields.Select(x => x.GetQueryPart()).ToArray()));
            if (Fields.Count > 0)
                sbSql.AppendFormat(",{0}", string.Join(",", Fields.Select(x => x.GetQueryPart()).ToArray()));

            sbSql.AppendFormat(" FROM {0}", TableName);

            if (criteria.Length > 0)
                sbSql.AppendFormat(" WHERE {0}", string.Join(" AND ", (from k in criteria select k.ToSql()).ToArray()));

            if (!string.IsNullOrEmpty(orderBy))
                sbSql.AppendFormat(" ORDER BY {0}",  orderBy);

            IList<T> data = CBO.FillCollection<T>(SqlHelper.ExecuteReader(ConnString, CommandType.Text, sbSql.ToString(), null));

            // satisfy HasMany for all objects at once
            // HasMany only works with identity primary keys for now
            foreach (var rel in ManyRelations) {
                var ids = new Dictionary<int, object>();
                foreach (var item in data) {
                    ids[(int)PrimaryKey.KeyFields.First().Property.GetValue(item, null)] = item;

                    // also initialize
                    rel.Property.SetValue(ids[(int)PrimaryKey.KeyFields.First().Property.GetValue(item, null)], Activator.CreateInstance(typeof(List<>).MakeGenericType(rel.Property.PropertyType.GetGenericArguments()[0])), null);
                }

                if (ids.Count == 0)
                    continue;

                var hasManyCol = ((HasManyAttribute)rel.Property.GetCustomAttributes(false).First(x => x is HasManyAttribute)).ForeignKeyColumnName;
                var hasManyOrderBy = ((HasManyAttribute)rel.Property.GetCustomAttributes(false).First(x => x is HasManyAttribute)).OrderBy;

                // get the GetAllByProperty for the relation, so we call it via reflection
                var method = rel.Property.PropertyType.GetGenericArguments()[0].GetMethod("GetAllByProperty", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                var subItems = method.Invoke(null, new object[] { 
                    hasManyOrderBy, 
                    null,
                    new IRestriction[] { new In(hasManyCol, ids.Keys) } });

                // we have the list of subitems, load it into the list of items
                foreach (var sub in (IEnumerable)subItems) {
                    var parentId = (int?)sub.GetType().GetProperty(hasManyCol).GetValue(sub, null);
                    if (parentId.HasValue)
                        (rel.Property.GetValue(ids[parentId.Value], null) as IList).Add(sub);
                }
            }

            return data;
        }


        public void ExecuteDelete(object obj)
        {
            string sql = string.Format("DELETE FROM {0} WHERE {1}", TableName, JoinConditions(obj, " AND "));
            SqlHelper.ExecuteNonQuery(ConnString, CommandType.Text, sql, null);
        }


        public void DeleteAllByProperty(IRestriction[] criteria)
        {
            StringBuilder sbSql = new StringBuilder();
            sbSql.AppendFormat("DELETE FROM {0} ", TableName);

            if (criteria.Length > 0)
                sbSql.AppendFormat(" WHERE {0}", string.Join(" AND ", (from k in criteria select k.ToSql()).ToArray()));

            SqlHelper.ExecuteNonQuery(ConnString, CommandType.Text, sbSql.ToString(), null);
        }


        string SqlUpdate(object obj)
        {
            if (PrimaryKey.IsIdentity)
                return ((int)PrimaryKey.KeyFields.First().Property.GetValue(obj, null)) > 0 ? SqlEdit(obj) : SqlAdd(obj);

            return @"
                IF EXISTS (SELECT 1 FROM " + TableName + " Where " + JoinConditions(obj, " AND ") + @")
                    " + SqlEdit(obj) + @"
                ELSE
                    " + SqlAdd(obj) + @"
                ";
        }

        string SqlAdd(object obj)
        {
            StringBuilder sbSql = new StringBuilder();
            if (PrimaryKey.IsIdentity) {
                sbSql.AppendFormat("INSERT INTO {0} ({1}) VALUES ", TableName, string.Join(",", Fields.Where(x => x.CanInsert).Select(x => x.GetQueryPart()).ToArray()));
            } else {
                // append insert check to avoid duplicates
                sbSql.AppendFormat("IF NOT EXISTS(Select 1 FROM {0} WHERE ", TableName);
                foreach (var key in PrimaryKey.KeyFields.Select(x => x.Property))
                    sbSql.AppendFormat("{0}{1} AND ", key.Name, SqlEquals(key.GetValue(obj, null)));
                sbSql.Remove(sbSql.Length - " AND ".Length, " AND ".Length);

                sbSql.AppendFormat(") INSERT INTO {0} (", TableName);
                sbSql.Append(string.Join(",", PrimaryKey.KeyFields.Select(x => x.GetQueryPart()).ToArray()));
                if (Fields.Count > 0)
                    sbSql.AppendFormat(",{0}", string.Join(",", Fields.Where(x => x.CanInsert).Select(x => x.GetQueryPart()).ToArray()));
                sbSql.Append(") VALUES ");
            }

            // append input
            sbSql.Append('(');
            if (!PrimaryKey.IsIdentity) {
                foreach (var pKey in PrimaryKey.KeyFields.Select(x => x.Property))
                    sbSql.AppendFormat("{0},", EncodeSql(pKey.GetValue(obj, null)));
            }

            foreach (var field in Fields.Where(x=>x.CanInsert))
                sbSql.AppendFormat("{0},", EncodeSql(field.Property.GetValue(obj, null)));

            // remove last coma
            if (sbSql[sbSql.Length - 1] == ',')
                sbSql.Remove(sbSql.Length - 1, 1);

            sbSql.Append(')');
            sbSql.Append("\n Select SCOPE_IDENTITY()");

            return sbSql.ToString();
        }

        string SqlEdit(object obj)
        {
            StringBuilder sbSql = new StringBuilder();
            sbSql.AppendFormat("UPDATE {0} SET ", TableName);

            // append input
            foreach (var field in Fields.Where(x=>x.CanUpdate))
                sbSql.AppendFormat("{0}={1},", field.GetQueryPart(), EncodeSql(field.Property.GetValue(obj, null)));

            // remove last coma
            if (sbSql[sbSql.Length - 1] == ',')
                sbSql = sbSql.Remove(sbSql.Length - 1, 1);

            // append wheere
            sbSql.Append(" WHERE ");
            sbSql.Append(JoinConditions(obj, " AND "));

            if (PrimaryKey.IsIdentity)
                sbSql.AppendFormat("\n \n SELECT {0}", PrimaryKey.KeyFields.First().Property.GetValue(obj, null));

            return sbSql.ToString();
        }

        string JoinConditions(object obj, string sep)
        {
            StringBuilder sbCond = new StringBuilder();
            foreach (var key in PrimaryKey.KeyFields.Select(x => x.Property))
                sbCond.AppendFormat("{0}{1}{2}", key.Name, SqlEquals(key.GetValue(obj, null)), sep);

            sbCond = sbCond.Remove(sbCond.Length - sep.Length, sep.Length);
            return sbCond.ToString();
        }

        static string SqlEquals(object val)
        {
            if (ConvertUtils.IsNullable(val) && val == null)
                return " IS NULL";

            if (typeof(IEnumerable).IsAssignableFrom(val.GetType()) && val.GetType() != typeof(string)) {
                var sb = new StringBuilder();
                sb.Append(" IN (");
                foreach (var o in (IEnumerable)val)
                    sb.AppendFormat("{0},", EncodeSql(o));
                if (sb[sb.Length - 1] == ',')
                    sb.Remove(sb.Length - 1, 1);
                sb.Append(")");

                return sb.ToString();
            }

            return "=" + EncodeSql(val);
        }

        public static string EncodeSql(object data)
        {
            Type dataType;
            try {
                dataType = data.GetType();
            } catch {
                return "NULL";
            }

            if (dataType == typeof(string)) {
                return string.Format("N'{0}'", data.ToString().Replace("'", "''"));
            } else if (dataType == typeof(DateTime)) {
                return string.Format("N'{0}'", ((DateTime)data).ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture).Replace("'", "''"));
            } else if (dataType == typeof(DateTime?)) {
                if ((DateTime?)data == null)
                    return "NULL";
                return string.Format("N'{0}'", ((DateTime)data).ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture).Replace("'", "''"));
            } else if (dataType == typeof(DateTimeOffset)) {
                return string.Format("N'{0}'", ((DateTimeOffset)data).ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture).Replace("'", "''"));
            } else if (dataType == typeof(DateTimeOffset?)) {
                if ((DateTimeOffset?)data == null)
                    return "NULL";
                return string.Format("N'{0}'", ((DateTimeOffset)data).ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture).Replace("'", "''"));
            } else if (dataType == typeof(SqlDateTime)) {
                return string.Format("N'{0}'", ((SqlDateTime)data).Value.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture).Replace("'", "''"));
            } else if (dataType == typeof(Guid)) {
                return string.Format("N'{0}'", data.ToString().Replace("'", "''"));
            } else if (dataType == typeof(Guid?)) {
                if (((Guid?)data).HasValue)
                    return string.Format("N'{0}'", data.ToString().Replace("'", "''"));
                return "NULL";
            } else if (dataType == typeof(int)) {
                if ((int)data == int.MinValue)
                    return "NULL";
                return data.ToString();
            } else if (dataType == typeof(int?)) {
                if (((int?)data).HasValue)
                    return ((int?)data).Value.ToString();
                return "NULL";
            } else if (dataType == typeof(long)) {
                if ((long)data == long.MinValue)
                    return "NULL";
                return data.ToString();
            } else if (dataType == typeof(long?)) {
                if (((long?)data).HasValue)
                    return ((long?)data).Value.ToString();
                return "NULL";
            } else if (dataType == typeof(double)) {
                if ((double)data == double.MinValue)
                    return "NULL";
                return data.ToString();
            } else if (dataType == typeof(double?)) {
                if (((double?)data).HasValue)
                    return ((double?)data).Value.ToString();
                return "NULL";
            } else if (dataType == typeof(bool)) {
                return (bool)data ? "1" : "0";
            } else if (dataType == typeof(bool?)) {
                if (((bool?)data).HasValue)
                    return ((bool?)data).Value ? "1" : "0";
                return "NULL";
            } else if (dataType.IsEnum) {
                return ((int)data).ToString();
            }

            throw new Exception("Input data type not supported for update");
        }

    }
}
