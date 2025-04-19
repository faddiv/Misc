namespace Microsoft.Extensions.DependencyInjection;

public abstract class FactoryClass
{
    public abstract Type ServiceType { get; }
    public abstract Type[] ParameterTypes { get; }
    public abstract object CreateInstance(ReadOnlySpan<object?> parameters);
}
