using System.Text.RegularExpressions;

namespace IndexHtmlReWriter.IndexHtmlTransformer
{
    public class BaseTagTransformer : ICachedFallbackFileTransformer
    {
        private static readonly Regex _regex = new(@"<base([^>]*)>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public bool PerRequestTransformer => false;

        public Task TransformAsync(FallbackFileTransformContext context)
        {
            var pathBase = context.HttpContext.Request.PathBase;
            pathBase = pathBase.Add("/");
            context.Content = _regex.Replace(context.Content, match =>
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
            return Task.CompletedTask;
        }
    }
}
