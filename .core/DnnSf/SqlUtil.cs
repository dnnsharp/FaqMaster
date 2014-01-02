using System;
using System.Data.SqlTypes;
using System.Globalization;

namespace DnnSharp.FaqMaster.Core.DnnSf
{
    public static class SqlUtil
    {
        public static string ReplaceDbOwnerAndPrefix(string sql)
        {
            return sql.Replace("{databaseOwner}", DotNetNuke.Common.Utilities.Config.GetDataBaseOwner())
                      .Replace("{objectQualifier}", DotNetNuke.Common.Utilities.Config.GetObjectQualifer());
        }

        public static string EncodeSql(object data, bool appendQuotes = true)
        {
            Type dataType;
            try {
                dataType = data.GetType();
            } catch {
                return "NULL";
            }

            if (dataType == typeof(string)) {
                return string.Format(appendQuotes ? "N'{0}'" : "{0}", data.ToString().Replace("'", "''"));
            } else if (dataType == typeof(Guid)) {
                return string.Format(appendQuotes ? "N'{0}'" : "{0}", data.ToString());
            } else if (dataType == typeof(DateTime)) {
                return string.Format(appendQuotes ? "N'{0}'" : "{0}", ((DateTime)data).ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture).Replace("'", "''"));
            } else if (dataType == typeof(SqlDateTime)) {
                return string.Format(appendQuotes ? "N'{0}'" : "{0}", ((SqlDateTime)data).Value.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture).Replace("'", "''"));
            } else if (dataType == typeof(int)) {
                if ((int)data == int.MinValue)
                    return "NULL";
                return data.ToString();
            } else if (dataType == typeof(int?)) {
                if (((int?)data).HasValue)
                    return ((int?)data).Value.ToString();
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
            }

            throw new Exception("Input data type not supported for update");
        }
    }
}
