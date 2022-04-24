namespace IndexHtmlReWriter.IndexHtmlTransformer
{
    public interface IPerRequestFallbackFileTransformer
    {
        ValueTask<string> TransformAsync(string content, HttpContext httpContext);
    }
}
