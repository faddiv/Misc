using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Blazorify.Utilities.Styling
{
    public class ThreadsafeCssBuilderCache : ICssBuilderCache
    {
        private static readonly ConcurrentDictionary<Type, ProcessObjectDelegate> _valueExtractors = new ConcurrentDictionary<Type, ProcessObjectDelegate>();
        private static readonly ConcurrentDictionary<Enum, string> _enumName = new ConcurrentDictionary<Enum, string>(new EnumEqualityComparer());

        public ProcessObjectDelegate GetOrAdd(Type type, Func<Type, ProcessObjectDelegate> create)
        {
            return _valueExtractors.GetOrAdd(type, create);
        }

        public string GetOrAdd(Enum value, Func<Enum, string> create)
        {
            return _enumName.GetOrAdd(value, create);
        }

        public static void ClearCache()
        {
            _valueExtractors.Clear();
        }

        /// <summary>
        /// Different type of enums with the same values maybe not equals but they have the same hashcode. This comparer ensures the different hashcode.
        /// </summary>
        private class EnumEqualityComparer : IEqualityComparer<Enum>
        {
            public bool Equals(Enum x, Enum y)
            {
                return object.Equals(x, y);
            }

            public int GetHashCode(Enum value)
            {
                int hashCode = -1959444751;
                hashCode = hashCode * -1521134295 + value.GetType().GetHashCode();
                hashCode = hashCode * -1521134295 + value.GetHashCode();
                return hashCode;
            }
        }
    }
}
