namespace Microsoft.Extensions.DependencyInjection;

public abstract class FactoryClass<TDependency1, TDependency2, TDependency3, TService> : FactoryClass
    where TService : notnull
{
    public override Type ServiceType => typeof(TService);

    public override Type[] ParameterTypes { get; } = [typeof(TDependency1), typeof(TDependency2), typeof(TDependency3)];

    public abstract TService CreateInstance(TDependency1 dependency1, TDependency2 dependency2, TDependency3 dependency3);

    public override object CreateInstance(ReadOnlySpan<object?> parameters)
    {
        if (parameters.Length != 3)
        {
            throw new InvalidOperationException();
        }

        var dependency1 = (TDependency1)parameters[0]!;
        var dependency2 = (TDependency2)parameters[1]!;
        var dependency3 = (TDependency3)parameters[2]!;
        return CreateInstance(dependency1, dependency2, dependency3);
    }
}
