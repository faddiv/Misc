namespace Microsoft.Extensions.DependencyInjection.ServiceLookup;

internal abstract class FuncFactoryCallSite : ServiceCallSite
{
    public ServiceCallSite[] ParameterCallSites { get; }
    public override Type ServiceType { get; }
    public override CallSiteKind Kind => CallSiteKind.FuncFactory;

    protected FuncFactoryCallSite(
        ResultCache cache,
        Type serviceType,
        ServiceCallSite[] parameterCallSites) : base(cache)
    {
        ServiceType = serviceType;
        ParameterCallSites = parameterCallSites;
    }

    public abstract object Resolve(object?[] parameters);
}

internal sealed class Func0FactoryCallSite<TResult>(
    Delegate function,
    ResultCache cache,
    ServiceCallSite[] parameterCallSites)
    : FuncFactoryCallSite(cache, typeof(TResult), parameterCallSites)
{
    private readonly Func<TResult> _function = (Func<TResult>)function;

    public override object Resolve(object?[] parameters)
    {
        return _function()!;
    }

    public override Type ImplementationType => typeof(TResult);
}

internal sealed class Func1FactoryCallSite<TDep0, TResult>(
    Delegate function,
    ResultCache cache,
    ServiceCallSite[] parameterCallSites)
    : FuncFactoryCallSite(cache, typeof(TResult), parameterCallSites)
{
    private readonly Func<TDep0, TResult> _function = (Func<TDep0, TResult>)function;

    public override object Resolve(object?[] parameters)
    {
        return _function((TDep0)parameters[0]!)!;
    }

    public override Type ImplementationType => typeof(TResult);
}

internal sealed class Func2FactoryCallSite<TDep0, TDep1, TResult>(
    Delegate function,
    ResultCache cache,
    ServiceCallSite[] parameterCallSites)
    : FuncFactoryCallSite(cache, typeof(TResult), parameterCallSites)
{
    private readonly Func<TDep0, TDep1, TResult> _function = (Func<TDep0, TDep1, TResult>)function;

    public override object Resolve(object?[] parameters)
    {
        return _function((TDep0)parameters[0]!, (TDep1)parameters[1]!)!;
    }

    public override Type ImplementationType => typeof(TResult);
}
