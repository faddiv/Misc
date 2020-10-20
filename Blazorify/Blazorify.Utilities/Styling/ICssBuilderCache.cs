using System;

namespace Blazorify.Utilities.Styling
{
    public interface ICssBuilderCache
    {
        ProcessObjectDelegate GetOrAdd(Type type, Func<Type, ProcessObjectDelegate> create);
    }
}
