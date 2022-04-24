namespace IndexHtmlReWriter.IndexHtmlTransformer
{
    public static class ServiceCollectionExtensions
    {
        public static FallbackToTransformedFileBuilder AddFallbackToTransformedFile(this IServiceCollection services, Action<FallbackOptions>? configure = null)
        {
            services.Configure(configure ?? ((options) => { }));
            return new FallbackToTransformedFileBuilder(services);
        }
    }
}
