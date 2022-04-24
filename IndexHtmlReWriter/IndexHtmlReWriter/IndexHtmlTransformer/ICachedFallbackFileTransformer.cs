namespace IndexHtmlReWriter.IndexHtmlTransformer
{
    public interface ICachedFallbackFileTransformer
    {
        ValueTask<string> TransformAsync(string content, HttpContext httpContext);
    }
}
