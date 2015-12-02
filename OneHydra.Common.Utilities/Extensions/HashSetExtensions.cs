using System;
using System.Collections.Generic;


namespace OneHydra.Common.Utilities.Extensions
{
    public static class HashSetExtensions
    {
        public static void AddIfNotExists<T>(this HashSet<T> theHashSet, T theValue)
        {
            if (!theHashSet.Contains(theValue))
            {
                theHashSet.Add(theValue);
            }
        }
    }
}
