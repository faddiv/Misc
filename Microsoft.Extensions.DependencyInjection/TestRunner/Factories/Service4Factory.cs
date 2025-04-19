using Microsoft.Extensions.DependencyInjection;

namespace TestRunner;

internal class Service4Factory : FactoryClass<IService4>
{
    public override IService4 CreateInstance()
    {
        return new Service4Implementation();
    }
}
