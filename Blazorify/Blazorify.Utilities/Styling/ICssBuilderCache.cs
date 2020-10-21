using System;

namespace Blazorify.Utilities.Styling
{
    public interface ICssBuilderCache
    {
        ProcessObjectDelegate GetOrAdd(Type type, Func<Type, ProcessObjectDelegate> create);

        string GetOrAdd(Enum value, Func<Enum, string> create);
    }
}
