using System;
using System.Data;
using System.Data.SqlTypes;

namespace OneHydra.Common.Utilities.Extensions
{

    // ReSharper disable once InconsistentNaming
    public static class IDataReaderExtensions
    {
        public static string GetStringIfNotNull(this IDataReader theReader, int index)
        {
            return theReader.GetValue(index) != DBNull.Value ? theReader.GetString(index) : null;
        }

        public static int? GetInt32IfNotNull(this IDataReader theReader, int index)
        {
            var value = theReader.GetValue(index);
            return value != DBNull.Value ? (int)value : new int?();
        }

        public static decimal? GetDecimalIfNotNull(this IDataReader theReader, int index)
        {
            var value = theReader.GetValue(index);
            return value != DBNull.Value ? (decimal)value : new decimal?();
        }

        public static bool GetBoolIfNotNull(this IDataReader theReader, int index)
        {
            var value = theReader.GetValue(index);
            return value != DBNull.Value && bool.Parse(value.ToString());
        }

        public static DateTime? GetDateTimeIfNotNull(this IDataReader theReader, int index)
        {
            var value = theReader.GetValue(index);
            return value != DBNull.Value ? value.CastTo<DateTime?>() : null;
        }

        public static string GetStringIfNotNull(this IDataReader theReader, string columnName)
        {
            return theReader.GetValue(theReader.GetOrdinal(columnName)) != DBNull.Value ? theReader.GetString(theReader.GetOrdinal(columnName)) : string.Empty;
        }

        public static string GetString(this IDataReader theReader, string columnName)
        {
            return theReader.GetString(theReader.GetOrdinal(columnName));
        }

        public static int GetInt32(this IDataReader theReader, string columnName)
        {
            return theReader.GetInt32(theReader.GetOrdinal(columnName));
        }

        public static bool IsDbNull(this IDataReader theReader, string columnName)
        {
            return theReader.GetValue(theReader.GetOrdinal(columnName)) == DBNull.Value;
        }

        public static SqlDecimal GetSqlDecimal(this IDataReader theReader, string columnName)
        {
            return theReader.GetDecimal(theReader.GetOrdinal(columnName));
        }

        public static SqlDateTime GetSqlDateTime(this IDataReader theReader, string columnName)
        {
            return theReader.GetDateTime(theReader.GetOrdinal(columnName));
        }

        public static int GetInt32IfNotNull(this IDataReader theReader, string columnName, int defaultValue)
        {
            var index = theReader.GetOrdinal(columnName);
            var value = theReader.GetValue(index);
            return (value != DBNull.Value ? (int)value : defaultValue);
        }

        public static int GetInt32IfNotNull(this IDataReader theReader, string columnName)
        {
            var index = theReader.GetOrdinal(columnName);
            var value = theReader.GetValue(index);
            return (value != DBNull.Value ? (int)value : int.MinValue);
        }

        public static bool GetBoolean(this IDataReader theReader, string columnName)
        {
            var returnValue = false;
            var index = theReader.TryGetOrdinal(columnName);
            if (index > -1)
            {
                var value = theReader.GetValue(index);
                returnValue = (value != DBNull.Value && (bool)value);
            }
            return returnValue;
        }

        public static DateTime GetDateTime(this IDataReader theReader, string columnName)
        {
            var returnValue = DateTime.MinValue;
            var index = theReader.TryGetOrdinal(columnName);
            if (index > -1)
            {
                var value = theReader.GetValue(index);
                returnValue = (value != DBNull.Value ? Convert.ToDateTime(value) : DateTime.MinValue);
            }
            return returnValue;
        }

        public static double GetDouble(this IDataReader theReader, string columnName)
        {
            var returnValue = Double.MinValue;
            var index = theReader.TryGetOrdinal(columnName);
            if (index > -1)
            {
                var value = theReader.GetValue(index);
                returnValue = (value != DBNull.Value ? Convert.ToDouble(value) : Double.MinValue);
            }
            return returnValue;
        }

        public static decimal GetDecimal(this IDataReader theReader, string columnName)
        {
            var returnValue = decimal.MinValue;
            var index = theReader.TryGetOrdinal(columnName);
            if (index > -1)
            {
                var value = theReader.GetValue(index);
                returnValue = (value != DBNull.Value ? Convert.ToDecimal(value) : decimal.MinValue);
            }
            return returnValue;
        }


        public static int TryGetOrdinal(this IDataReader theReader, string columnName)
        {
            int ordinal;
            try
            {
                ordinal = theReader.GetOrdinal(columnName);
            }
            catch (IndexOutOfRangeException)
            {
                ordinal = -1;
            }
            return ordinal;
        }


        public static Type GetFieldType(this IDataReader theReader, string columnName)
        {
            var index = theReader.TryGetOrdinal(columnName);
            return (index > -1 ? theReader.GetFieldType(index) : null);
        }
    }
}
