using MediatR;

namespace ViteCommerce.Api.Configurations;

public static class MediatrServiceCollectionExtensions
{
    public static void AddMediatrWithPipelines(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(MediatrServiceCollectionExtensions).Assembly);
            cfg.AddOpenBehavior(typeof(PipelineBehaviors.ValidationBehaviorMediatr2<,>));
        });
        //services.AddMediatR(typeof(MediatrServiceCollectionExtensions).Assembly);
        //services.AddPipeline(typeof(PipelineBehaviors.ValidationBehaviorMediatr<,>));
    }

    private static void AddPipeline(this IServiceCollection services, Type type)
    {

        services.AddScoped(typeof(MediatR.IPipelineBehavior<,>), type);
    }
}
