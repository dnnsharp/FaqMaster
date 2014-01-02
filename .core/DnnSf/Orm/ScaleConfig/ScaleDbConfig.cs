using DnnSharp.FaqMaster.Core.DnnSf.Orm.ActiveTable;
using Microsoft.ApplicationBlocks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DnnSharp.FaqMaster.Core.DnnSf.Orm.ScaleConfig
{
    public class ScaleDbConfig
    {
        public string TableName { get; set; }

        public ScaleDbConfig(string tableName)
        {
            TableName = DotNetNuke.Common.Utilities.Config.GetDataBaseOwner() + '[' +
                DotNetNuke.Common.Utilities.Config.GetObjectQualifer() + tableName.Trim('[', ']') + ']';

            // initialize all properties to their default
            foreach (var field in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)) {

                var attr = field.GetCustomAttributes(false).FirstOrDefault(x => x is PropertyAttribute) as PropertyAttribute;
                if (attr == null)
                    continue;

                if (!typeof(ISetting).IsAssignableFrom(field.PropertyType))
                    throw new ArgumentException("Setting " + field.Name + " should be a Setting object");

                var setting = Activator.CreateInstance(field.PropertyType) as ISetting;
                setting.Name = field.Name;
                if (attr.Default != null) {
                    if (field.PropertyType.GetGenericArguments()[0] == attr.Default.GetType()) {
                        setting.ValueObj = attr.Default;
                    } else {
                        setting.ValueObj = Convert.ChangeType(attr.Default, attr.Default.GetType());
                    }
                }
                field.SetValue(this, setting, null);
            }
        }

        public virtual void Load()
        {
            IDictionary<string, object> sectionData = ParseSectionAttributes();

            var sbSql = new StringBuilder();
            sbSql.AppendFormat("SELECT * FROM {0} Where ", TableName);
            foreach (var s in sectionData)
                sbSql.AppendFormat("{0} AND ", new Eq(s.Key, s.Value).ToSql());
            sbSql.Remove(sbSql.Length - 5, 4);

            using (var dr = SqlHelper.ExecuteReader(DotNetNuke.Common.Utilities.Config.GetConnectionString(), System.Data.CommandType.Text, sbSql.ToString())) {
                while (dr.Read()) {
                    var name = dr["Name"].ToString();
                    var field = (from f in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                 where f.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && typeof(ISetting).IsAssignableFrom(f.PropertyType)
                                 select f).FirstOrDefault();

                    if (field == null)
                        continue;

                    var setting = Activator.CreateInstance(field.PropertyType) as ISetting;
                    setting.Name = name;
                    setting.ValueObj = dr["Value"] is DBNull ? null : dr["Value"];
                    setting.CanOverride = (bool)dr["CanOverride"];
                    setting.Inherit = (bool)dr["Inherit"];
                    setting.LastModified = (DateTime)dr["LastModified"];
                    setting.LastModifiedBy = dr["LastModifiedBy"] is DBNull ? null : (int?)dr["LastModifiedBy"];
                    field.SetValue(this, setting, null);
                }
            }

            //var dr = SqlHelper.ExecuteReader(DotNetNuke.Common.Utilities.Config.GetConnectionString(), System.Data.CommandType.Text, sbSql.ToString());
            //foreach (var setting in DotNetNuke.Common.Utilities.CBO.FillCollection<Setting>(dr)) {
            //    var field = (from f in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            //                 where f.PropertyType == typeof(Setting) && f.Name.Equals(setting.Name, StringComparison.OrdinalIgnoreCase)
            //                 select f).FirstOrDefault();
            //    if (field != null)
            //        field.SetValue(this, setting, null);
            //}
        }

        public virtual void Save()
        {
            IDictionary<string, object> sectionData = ParseSectionAttributes();

            var sbSql = new StringBuilder();
            sbSql.AppendLine("BEGIN TRANSACTION");

            // now iterate all properties and update each one
            foreach (var field in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)) {

                var attr = field.GetCustomAttributes(false).FirstOrDefault(x => x is PropertyAttribute) as PropertyAttribute;
                if (attr == null)
                    continue;

                if (!typeof(ISetting).IsAssignableFrom(field.PropertyType))
                    throw new ArgumentException("Setting " + field.Name + " should be a Setting object");

                var setting = field.GetValue(this, null) as ISetting;

                sbSql.AppendFormat("IF NOT EXISTS(Select 1 FROM {0} WHERE ", TableName);
                foreach (var s in sectionData)
                    sbSql.AppendFormat("{0} AND ", new Eq(s.Key, s.Value).ToSql());
                sbSql.AppendFormat("Name={0} ", SqlUtil.EncodeSql(setting.Name));

                // this is the insert statement
                sbSql.AppendFormat(") INSERT INTO {0} ({1},Name,Value,CanOverride,Inherit,LastModified,LastModifiedBy) VALUES (", TableName, string.Join(",", sectionData.Keys.ToArray()));
                foreach (var s in sectionData)
                    sbSql.AppendFormat("{0},", SqlUtil.EncodeSql(s.Value));
                sbSql.AppendFormat("{0},{1},{2},{3},{4},{5})",
                    SqlUtil.EncodeSql(setting.Name), SqlUtil.EncodeSql(setting.ToString()),
                    SqlUtil.EncodeSql(setting.CanOverride), SqlUtil.EncodeSql(setting.Inherit),
                    SqlUtil.EncodeSql(setting.LastModified), SqlUtil.EncodeSql(setting.LastModifiedBy));

                // folowed by the update
                sbSql.AppendFormat("ELSE UPDATE {0} SET ", TableName);
                foreach (var s in sectionData)
                    sbSql.AppendFormat("{0}={1},", s.Key, SqlUtil.EncodeSql(s.Value));
                sbSql.AppendFormat("Name={0},Value={1},CanOverride={2},Inherit={3},LastModified={4},LastModifiedBy={5}\n\n",
                    SqlUtil.EncodeSql(setting.Name), SqlUtil.EncodeSql(setting.ToString()),
                    SqlUtil.EncodeSql(setting.CanOverride), SqlUtil.EncodeSql(setting.Inherit),
                    SqlUtil.EncodeSql(setting.LastModified), SqlUtil.EncodeSql(setting.LastModifiedBy));

                sbSql.Append("WHERE ");
                foreach (var s in sectionData)
                    sbSql.AppendFormat("{0} AND ", new Eq(s.Key, s.Value).ToSql());
                sbSql.AppendFormat("Name={0} ", SqlUtil.EncodeSql(setting.Name));

            }

            sbSql.AppendLine("COMMIT");

            SqlHelper.ExecuteNonQuery(DotNetNuke.Common.Utilities.Config.GetConnectionString(), System.Data.CommandType.Text, sbSql.ToString());

        }

        public static void Delete(string tableName, params IRestriction[] criteria)
        {
            var sb = string.Format(" DELETE FROM {0} WHERE {1}", tableName, string.Join(" AND ", (from k in criteria select k.ToSql()).ToArray()));
            SqlHelper.ExecuteNonQuery(DotNetNuke.Common.Utilities.Config.GetConnectionString(), System.Data.CommandType.Text, sb.ToString());
        }

        IDictionary<string, object> ParseSectionAttributes()
        {
            IDictionary<string, object> sectionData = new Dictionary<string, object>();

            // extract section attributes
            foreach (var field in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                var attr = field.GetCustomAttributes(false).FirstOrDefault(x => x is SectionAttribute) as SectionAttribute;
                if (attr == null)
                    continue;

                sectionData[field.Name] = field.GetValue(this, null);
            }

            return sectionData;
        }
    }
}
