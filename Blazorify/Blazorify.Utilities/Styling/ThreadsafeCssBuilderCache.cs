using System;
using System.Collections.Concurrent;

namespace Blazorify.Utilities.Styling
{
    public class ThreadsafeCssBuilderCache : ICssBuilderCache
    {
        private static readonly ConcurrentDictionary<Type, ProcessObjectDelegate> _valueExtractors = new ConcurrentDictionary<Type, ProcessObjectDelegate>();

        public ProcessObjectDelegate GetOrAdd(Type type, Func<Type, ProcessObjectDelegate> create)
        {
            return _valueExtractors.GetOrAdd(type, create);
        }

        public static void ClearCache()
        {
            _valueExtractors.Clear();
        }
    }
}
