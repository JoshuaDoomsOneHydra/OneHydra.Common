using System.Globalization;

namespace OneHydra.Common.Utilities.Extensions
{
    public static class IntegerExtensions
    {
        public static string ForSql(this int? theInt)
        {
            return !theInt.HasValue ? "NULL" : ((int)theInt).ForSql();
        }

        public static string ForSql(this int theInt)
        {
            return theInt.ToString(CultureInfo.InvariantCulture);
        }

        public static string ForSql(this long theInt)
        {
            return theInt.ToString(CultureInfo.InvariantCulture);
        }
    }
}
