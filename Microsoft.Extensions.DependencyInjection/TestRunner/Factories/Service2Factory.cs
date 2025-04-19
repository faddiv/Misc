using Microsoft.Extensions.DependencyInjection;

namespace TestRunner;

internal class Service2Factory : FactoryClass<IService2>
{
    public override IService2 CreateInstance()
    {
        return new Service2Implementation();
    }
}
