using System.Text.RegularExpressions;

namespace IndexHtmlReWriter.IndexHtmlTransformer
{
    public class AuthenticatedTransformer : IPerRequestFallbackFileTransformer
    {
        private static readonly Regex _regex = new(@"<body([^>]*)>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public bool PerRequestTransformer => true;

        public Task TransformAsync(FallbackFileTransformContext context)
        {
            var isAuthenticated = context.HttpContext.User.Identity?.IsAuthenticated ?? false;
            if (isAuthenticated)
            {
                context.Content = _regex.Replace(context.Content, match =>
                {
                    return $"<body{match.Groups[1].ValueSpan} authenticated>";
                });
            }
            return Task.CompletedTask;
        }
    }
}
