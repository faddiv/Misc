using System.Text.RegularExpressions;

namespace IndexHtmlReWriter.IndexHtmlTransformer
{
    public class BaseTagTransformer : ICachedFallbackFileTransformer
    {
        private static readonly Regex _regex = new(@"<base([^>]*)>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public ValueTask<string> TransformAsync(string content, HttpContext httpContext)
        {
            var pathBase = httpContext.Request.PathBase;
            if(!pathBase.HasValue)
            {
                pathBase = "/";
            }
            var transformedContent = _regex.Replace(content, match =>
            {
                var value = match.Groups[1].ValueSpan;
                if (value.EndsWith("/"))
                {
                    value = value[0..^1];
                }
                value = value.Trim();

                if (value.IsEmpty)
                {
                    return $"<base href=\"{pathBase}\"/>";
                }

                // TODO Optimize
                var missing = true;
                var parts = value.ToString().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < parts.Length; i++)
                {
                    var attrValuePair = parts[i];
                    var eqIndex = attrValuePair.IndexOf('=');
                    if (eqIndex == -1)
                    {
                        eqIndex = attrValuePair.Length;
                    }
                    var attName = attrValuePair.Substring(0, eqIndex);
                    if (attName == "href")
                    {
                        parts[i] = $"href=\"{pathBase}\"";
                        missing = false;
                        break;
                    }
                }
                if (missing)
                {
                    return $"<base {value} href=\"{pathBase}\" />";
                }
                return $"<base {string.Join(" ", parts)} />";
            });
            return ValueTask.FromResult(transformedContent);
        }
    }
}
