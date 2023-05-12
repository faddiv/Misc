namespace ViteCommerce.Api.Configurations;

public static class MediatorServiceCollectionExtensions
{
    public static void AddMediatorWithPipelines(this IServiceCollection services)
    {
        services.AddMediator(opt =>
        {
            opt.Namespace = "GeneratedCode";
            opt.ServiceLifetime = ServiceLifetime.Transient;
        });
        //services.AddPipeline(typeof(PipelineBehaviors.DbContextBehavior<,>));
        services.AddPipeline(typeof(PipelineBehaviors.ValidationBehavior<,>));
    }

    private static void AddPipeline(this IServiceCollection services, Type type)
    {

        services.AddScoped(typeof(Mediator.IPipelineBehavior<,>), type);
    }
}
