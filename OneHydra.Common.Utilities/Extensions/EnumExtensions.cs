using System;
using System.ComponentModel;

namespace OneHydra.Common.Utilities.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return attribute == null ? value.ToString() : attribute.Description;
        }

        public static int ForSql(this Enum value)
        {
            return value.CastTo<int>();
        }

    }
}
