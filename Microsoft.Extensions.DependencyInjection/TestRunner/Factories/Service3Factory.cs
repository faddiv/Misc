using Microsoft.Extensions.DependencyInjection;

namespace TestRunner;

internal class Service3Factory : FactoryClass<IService4, IService3>
{
    public override IService3 CreateInstance(IService4 dependency1)
    {
        return new Service3Implementation(dependency1);
    }
}
