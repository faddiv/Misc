using Microsoft.Extensions.DependencyInjection;

namespace TestRunner;

internal class Service1Factory : FactoryClass<IService2, IService3, IService1>
{
    public override IService1 CreateInstance(IService2 dependency1, IService3 dependency2)
    {
        return new Service1Implementation(dependency1, dependency2);
    }
}
