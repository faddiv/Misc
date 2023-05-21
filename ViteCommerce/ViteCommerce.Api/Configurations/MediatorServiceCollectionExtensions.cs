using ViteCommerce.Api.PipelineBehaviors;

namespace ViteCommerce.Api.Configurations;

public static class MediatorServiceCollectionExtensions
{
    public static void AddMediatorWithPipelines(this IServiceCollection services)
    {
        services.AddMediatR(opt =>
        {
            opt.RegisterServicesFromAssembly(typeof(MediatorServiceCollectionExtensions).Assembly);

            //opt.AddOpenBehavior(typeof(DbContextBehavior<,>));
            opt.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
    }
}
