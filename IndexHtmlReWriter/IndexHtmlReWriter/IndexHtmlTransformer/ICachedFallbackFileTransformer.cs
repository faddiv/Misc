namespace IndexHtmlReWriter.IndexHtmlTransformer
{
    /// <summary>
    /// Trasformer that executes only if the fallback is not cached.
    /// </summary>
    public interface ICachedFallbackFileTransformer
    {
        /// <summary>
        /// Executes a transformation on the content of the static file.
        /// </summary>
        /// <param name="context">Contains the content of the static file and the http context.</param>
        /// <returns>A task that represents the completion of the transformation.</returns>
        Task TransformAsync(FallbackFileTransformContext context);
    }
}
