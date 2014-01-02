using Microsoft.ApplicationBlocks.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

namespace DnnSharp.FaqMaster.Core.DnnSf.Orm
{
    public static class DbUtil
    {
        public static List<string> GetTableFields(IDataReader dr)
        {
            List<string> cols = new List<string>();
            //tableName = dr.GetSchemaTable().TableName;
            try {
                for (int i = 0; i < 9999; i++)
                    cols.Add(dr.GetName(i));
            } catch (IndexOutOfRangeException) { }

            return cols;
        }

        public static List<string> GetTableFields(string connectionString, string query)
        {
            if (string.IsNullOrEmpty(connectionString))
                connectionString = DotNetNuke.Common.Utilities.Config.GetConnectionString();

            using (IDataReader dr = SqlHelper.ExecuteReader(connectionString, CommandType.Text, query)) {
                return GetTableFields(dr);
            }
        }

        public static string GetTableName(string query)
        {
            query = Regex.Replace(query, "\\(.*\\)", "", RegexOptions.Singleline);
            var tableName = Regex.Match(query, "FROM\\s+([^ ]+)", RegexOptions.IgnoreCase).Groups[1].Value;
            return tableName;
        }
    }
}
