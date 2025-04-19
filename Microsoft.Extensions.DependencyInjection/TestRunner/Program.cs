//#define CONSTRUCTOR_FACTORY

using Microsoft.Extensions.DependencyInjection;
using TestRunner;

var collection = new ServiceCollection();

#if CONSTRUCTOR_FACTORY
collection.AddScoped<IService1, Service1Implementation>();
collection.AddTransient<IService2, Service2Implementation>();
collection.AddScoped<IService3, Service3Implementation>();
collection.AddSingleton<IService4, Service4Implementation>();
#else
collection.AddScoped(new Service1Factory());
collection.AddTransient(new Service2Factory());
collection.AddScoped(new Service3Factory());
collection.AddSingleton(new Service4Factory());
#endif

var provider = collection.BuildServiceProvider(new ServiceProviderOptions()
{
    ValidateScopes = false,
    ValidateOnBuild = false
});

using (var serviceScope = provider.CreateScope())
{
    var sp = serviceScope.ServiceProvider;
    var service1 = sp.GetRequiredService<IService1>();
    service1.CallHelloWorld();
}

using (var serviceScope = provider.CreateScope())
{
    var sp = serviceScope.ServiceProvider;
    var service1 = sp.GetRequiredService<IService1>();
    service1.CallHelloWorld();
}

Console.WriteLine(@"Program finished");
