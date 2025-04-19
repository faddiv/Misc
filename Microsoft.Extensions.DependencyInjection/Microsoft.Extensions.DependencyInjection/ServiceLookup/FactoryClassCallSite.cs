namespace Microsoft.Extensions.DependencyInjection.ServiceLookup;

internal sealed class FactoryClassCallSite : ServiceCallSite
{
    public FactoryClass Factory { get; }
    public ServiceCallSite[] ParameterCallSites { get; }

    public FactoryClassCallSite(
        ResultCache cache,
        Type serviceType,
        FactoryClass factory,
        ServiceCallSite[] parameterCallSites) : base(cache)
    {
        Factory = factory;
        ParameterCallSites = parameterCallSites;
        ServiceType = serviceType;
    }

    private FactoryClassCallSite(
        ResultCache cache,
        Type serviceType,
        object serviceKey,
        FactoryClass factory,
        ServiceCallSite[] parameterCallSites) : base(cache)
    {
        Factory = factory;
        ParameterCallSites = parameterCallSites;
        ServiceType = serviceType;
    }

    public override Type ServiceType { get; }
    public override Type? ImplementationType => null;

    public override CallSiteKind Kind { get; } = CallSiteKind.FactoryClass;
}
