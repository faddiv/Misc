namespace IndexHtmlReWriter.IndexHtmlTransformer
{
    public class FallbackToTransformedFileBuilder
    {
        public FallbackToTransformedFileBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }

        public FallbackToTransformedFileBuilder WithBaseTagTransformer()
        {
            Services.AddSingleton<ICachedFallbackFileTransformer, BaseTagTransformer>();
            return this;
        }

        public FallbackToTransformedFileBuilder WithAuthenticatedTransformer()
        {
            Services.AddSingleton<IPerRequestFallbackFileTransformer, AuthenticatedTransformer>();
            return this;
        }

        public FallbackToTransformedFileBuilder WithCachedTransformer<TTransformer>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TTransformer : class, ICachedFallbackFileTransformer
        {
            Services.Add(new ServiceDescriptor(typeof(ICachedFallbackFileTransformer), typeof(TTransformer), lifetime));
            return this;
        }

        public FallbackToTransformedFileBuilder WithPerRequestTransformer<TTransformer>(ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TTransformer : class, IPerRequestFallbackFileTransformer
        {
            Services.Add(new ServiceDescriptor(typeof(IPerRequestFallbackFileTransformer), typeof(TTransformer), lifetime));
            return this;
        }
    }
}
