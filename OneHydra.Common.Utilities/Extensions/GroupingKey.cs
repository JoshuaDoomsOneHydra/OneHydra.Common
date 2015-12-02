using System;

namespace OneHydra.Common.Utilities.Extensions
{
    public class GroupingKey<T> : IEquatable<T>
    {
        private readonly Func<T, T, bool> _groupByFunction;
        private readonly Func<T, int> _getHashCodeFunction;
        private readonly T _record;

        public GroupingKey(T record, Func<T, T, bool> groupByFunction, Func<T, int> getHashCodeFunction)
        {
            _groupByFunction = groupByFunction;
            _getHashCodeFunction = getHashCodeFunction;
            _record = record;
        }

        public bool Equals(T other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _groupByFunction(_record, other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(T)) return false;
            return Equals((T)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return _getHashCodeFunction(_record);
            }
        }
    }
}
