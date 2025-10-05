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

internal sealed class Func1FactoryCallSite<TDep0, TResult> : FuncFactoryCallSite
{
    private readonly Func<TDep0, TResult> _function;

    public Func1FactoryCallSite(
        Delegate function,
        ResultCache cache,
        ServiceCallSite[] parameterCallSites) : base(cache, typeof(TResult), parameterCallSites)
    {
        _function = (Func<TDep0, TResult>)function;
    }
    public override object Resolve(object?[] parameters)
    {
        return _function((TDep0)parameters[0]!)!;
    }

    public override Type ImplementationType => typeof(TResult);
}
