namespace IndexHtmlReWriter.IndexHtmlTransformer
{
    /// <summary>
    /// Trasformer that executes on every call of the fallback.
    /// </summary>
    public interface IPerRequestFallbackFileTransformer
    {
        /// <summary>
        /// Executes a transformation on the content of the static file.
        /// </summary>
        /// <param name="context">Contains the content of the static file and the http context.</param>
        /// <returns>A task that represents the completion of the transformation.</returns>
        Task TransformAsync(FallbackFileTransformContext context);
    }
}
