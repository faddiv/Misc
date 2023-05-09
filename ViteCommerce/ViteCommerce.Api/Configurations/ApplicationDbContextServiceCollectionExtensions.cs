using Database;
using MongoDB.Driver;

namespace ViteCommerce.Api.Configurations;

public static class DatabaseServiceCollectionExtensions
{
    public static void AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IMongoClient>((sp) =>
        {
            var connectionString = configuration.GetConnectionString("MongoDB");
            var client = new MongoClient(connectionString);
            return client;
        });
        services.AddSingleton<IApplicationDbContext, ApplicationDbContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}
