namespace IndexHtmlReWriter.IndexHtmlTransformer
{
    public class FallbackFileTransformContext
    {
        public FallbackFileTransformContext(string content, HttpContext httpContext)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            HttpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
        }

        public string Content { get; set; }

        public HttpContext HttpContext { get; }
    }
}
