using System;
using System.Collections.Concurrent;

namespace Blazorify.Utilities.Styling
{
    public class ThreadsafeCssBuilderCache : ICssBuilderCache
    {
        public static ThreadsafeCssBuilderCache Instance = new ThreadsafeCssBuilderCache();

        private static readonly ConcurrentDictionary<Type, ProcessObjectDelegate> _valueExtractors = new ConcurrentDictionary<Type, ProcessObjectDelegate>();

        public ProcessObjectDelegate GetOrAdd(Type type, Func<Type, ProcessObjectDelegate> create)
        {
            return _valueExtractors.GetOrAdd(type, create);
        }
    }
}
