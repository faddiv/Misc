using System.Text.RegularExpressions;

namespace IndexHtmlReWriter.IndexHtmlTransformer
{
    public class AuthenticatedTransformer : IPerRequestFallbackFileTransformer
    {
        private static readonly Regex _regex = new(@"<body([^>]*)>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public ValueTask<string> TransformAsync(string content, HttpContext httpContext)
        {
            var isAuthenticated = httpContext.User.Identity?.IsAuthenticated ?? false;
            if (isAuthenticated)
            {
                var transformedContent = _regex.Replace(content, match =>
                {
                    return $"<body{match.ValueSpan} authenticated>";
                });
                return ValueTask.FromResult(transformedContent);
            }
            return ValueTask.FromResult(content);
        }
    }
}
