using System;

namespace OneHydra.Common.Utilities.Extensions
{
    public static class GuidExtensions
    {
        public static string ForSql(this Guid? theGuid)
        {
            return !theGuid.HasValue ? "NULL" : ((Guid)theGuid).ForSql();
        }

        public static string ForSql(this Guid theGuid)
        {
            if (theGuid == Guid.NewGuid())
            {
                return "NULL";
            }
            return "'" + theGuid + "'";
        }
    }
}
