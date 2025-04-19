namespace Microsoft.Extensions.DependencyInjection;

public abstract class FactoryClass<TService> : FactoryClass
    where TService : notnull
{
    public override Type ServiceType => typeof(TService);

    public override Type[] ParameterTypes { get; } = [];

    public abstract TService CreateInstance();

    public override object CreateInstance(ReadOnlySpan<object?> parameters)
    {
        if (parameters.Length != 0)
        {
            throw new InvalidOperationException();
        }

        return CreateInstance();
    }
}
