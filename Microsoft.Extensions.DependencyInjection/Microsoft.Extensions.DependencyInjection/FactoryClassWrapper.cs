using Microsoft.Extensions.DependencyInjection.ServiceLookup;

namespace Microsoft.Extensions.DependencyInjection;

internal class FactoryClassWrapper
{
    public static object? CreateInstance(
        FactoryClass factoryClass,
        object?[] dependencies)
    {
        var span = new ReadOnlySpan<object?>(dependencies);
        var instance = factoryClass.CreateInstance(span);
        return instance;
    }
}
