namespace Microsoft.Extensions.DependencyInjection.Tests;

public class FuncRegistrationAndResolution
{
    [Fact]
    public void CanRegisterFunc0()
    {
        Func<IRootService> factory =
            () => new RootService();
        var collection = new ServiceCollection();
        collection.AddSingletonFunction(factory);
        var provider = collection.BuildServiceProvider();
        var root = provider.GetRequiredService<IRootService>();
        Assert.IsType<RootService>(root);
    }

    [Fact]
    public void CanRegisterFunc1()
    {
        Func<Dependency0, IRootService> factory =
            (d0) => new RootService1(d0);
        var provider = CreateServiceProvider(collection =>
            collection.AddSingletonFunction(factory));
        var root = provider.GetRequiredService<IRootService>();
        Assert.IsType<RootService1>(root);
        var root2 = (RootService1)root;
        Assert.NotNull(root2.D0);
    }

    [Fact]
    public void CanRegisterFunc2()
    {
        Func<Dependency0, Dependency1, IRootService> factory =
            (d0, d1) => new RootService2(d0, d1);
        var provider = CreateServiceProvider(collection =>
            collection.AddSingletonFunction(factory));
        var root = provider.GetRequiredService<IRootService>();
        Assert.IsType<RootService2>(root);
        var root2 = (RootService2)root;
        Assert.NotNull(root2.D0);
        Assert.NotNull(root2.D1);
    }

    private static ServiceProvider CreateServiceProvider(Func<IServiceCollection, IServiceCollection> configure)
    {
        IServiceCollection collection = new ServiceCollection();
        collection.AddSingleton<Dependency0>();
        collection.AddSingleton<Dependency1>();
        collection = configure(collection);
        return collection.BuildServiceProvider();
    }

    public interface IRootService;

    public record RootService : IRootService;

    public record RootService1(Dependency0 D0) : IRootService;

    public record RootService2(Dependency0 D0, Dependency1 D1) : IRootService;

    public class Dependency0;

    public class Dependency1;
}
