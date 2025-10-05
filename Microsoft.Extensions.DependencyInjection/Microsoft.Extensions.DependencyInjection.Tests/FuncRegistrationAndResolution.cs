using System;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests;

public class FuncRegistrationAndResolution
{
    [Fact(Skip = "Not implemented")]
    public void CanRegisterFunc0()
    {
        Func<IRootService> factory = () => new RootService();
        var collection = new ServiceCollection();
        //collection.AddSingletonFunction(factory);
        var provider = collection.BuildServiceProvider();
        var root = provider.GetRequiredService<IRootService>();
        Assert.IsType<RootService>(root);
    }

    public interface IRootService;
    public class RootService : IRootService;
}
