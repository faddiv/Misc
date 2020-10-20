using System;
using System.Collections.Generic;

namespace Blazorify.Utilities.Styling
{
    public class ThreadUnsafeCssBuilderCache : ICssBuilderCache
    {
        public static ThreadUnsafeCssBuilderCache Instance = new ThreadUnsafeCssBuilderCache();

        private static readonly Dictionary<Type, ProcessObjectDelegate> _valueExtractors = new Dictionary<Type, ProcessObjectDelegate>();

        public ProcessObjectDelegate GetOrAdd(Type type, Func<Type, ProcessObjectDelegate> create)
        {
            if(!_valueExtractors.TryGetValue(type, out var method))
            {
                method = create(type);
                _valueExtractors.Add(type, method);
            }
            return method;
        }
    }
}
