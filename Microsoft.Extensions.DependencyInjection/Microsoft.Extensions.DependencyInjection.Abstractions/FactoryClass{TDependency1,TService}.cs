namespace Microsoft.Extensions.DependencyInjection;

public abstract class FactoryClass<TDependency1, TService> : FactoryClass
    where TService : notnull
{
    public override Type ServiceType => typeof(TService);

    public override Type[] ParameterTypes { get; } = [typeof(TDependency1)];

    public abstract TService CreateInstance(TDependency1 dependency1);

    public override object CreateInstance(ReadOnlySpan<object?> parameters)
    {
        if (parameters.Length != 1)
        {
            throw new InvalidOperationException();
        }

        var dependency1 = (TDependency1)parameters[0]!;
        return CreateInstance(dependency1);
    }
}
