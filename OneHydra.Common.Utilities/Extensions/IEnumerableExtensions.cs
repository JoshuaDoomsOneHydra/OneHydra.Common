using System;
using System.Collections.Generic;
using System.Linq;


namespace OneHydra.Common.Utilities.Extensions
{
    // ReSharper disable once InconsistentNaming
    public static class IEnumerableExtensions
    {
        public static IEnumerable<TO> AggregateRecords<T, TO>(this IEnumerable<T> theCollection, Func<T, T, bool> groupByFunction, Func<T, int> getHashCodeFunction, Func<IGrouping<GroupingKey<T>, T>, TO> aggregateFunction)
        {
            var grouped = theCollection.GroupBy(x=>CreateAggregationKey(x,groupByFunction, getHashCodeFunction));
            foreach (var record in grouped.Where(record => record.Any()))
            {
                yield return aggregateFunction(record);
            }
        }

        private static GroupingKey<T> CreateAggregationKey<T>(T record, Func<T, T, bool> groupByFunction, Func<T, int> getHashCodeFunction)
        {
            return new GroupingKey<T>(record, groupByFunction, getHashCodeFunction);
        }
        
    }
}
