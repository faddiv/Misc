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
    }
}
