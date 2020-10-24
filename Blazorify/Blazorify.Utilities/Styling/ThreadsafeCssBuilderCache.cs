using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Blazorify.Utilities.Styling
{
    /* I compared the Dictionary with the ConcurrentDictionary. 
     * It turns out on read, only 20% performace lost, so I dropped the thread unsafe variant.
     * I also experimented with Enum caching. With default Equality comparer it became twice as 
     * fast as the non cached version. With the custom Enum comparer it became four times faster.
     */
    public static class ThreadsafeCssBuilderCache
    {
        private static readonly ConcurrentDictionary<Type, ProcessObjectDelegate> _valueExtractors = new ConcurrentDictionary<Type, ProcessObjectDelegate>();
        private static readonly ConcurrentDictionary<Enum, string> _enumName = new ConcurrentDictionary<Enum, string>(new EnumEqualityComparer());

        public static ProcessObjectDelegate GetOrAdd(Type type, Func<Type, ProcessObjectDelegate> create)
        {
            return _valueExtractors.GetOrAdd(type, create);
        }

        public static string GetOrAdd(Enum value, Func<Enum, string> create)
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