using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace IndexHtmlReWriter.IndexHtmlTransformer
{
    public partial class FallbackApi
    {
        private const string _defaultContentType = "text/html; charset=utf-8";
        public ContentResult AuthenticatedScript(ClaimsPrincipal User)
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                return new ContentResult()
                {
                    Content = "document.body.setAttribute(\"authenticated\", \"\");",
                    ContentType = "text/javascript"
                };
            }
            return new ContentResult()
            {
                Content = "/* unauthenticated */",
                ContentType = "text/javascript"
            };
        }

        internal static async Task<string> FallbackResult(
            HttpContext httpContext,
            [FromServices] IEnumerable<IPerRequestFallbackFileTransformer> _transformers,
            [FromServices] IOptions<FallbackOptions> _options,
            [FromServices] IWebHostEnvironment _environment,
            [FromServices] IMemoryCache _memoryCache,
            [FromServices] ILogger<FallbackApi> _logger)
        {
            var content = await _memoryCache.GetOrCreateAsync(_options.Value.Key, async (ICacheEntry cacheEntry) =>
            {
                LogFallbackFileWasntFoundInCache(_logger, cacheEntry.Key, _options.Value.FallbackFile);
                var fileProvider = _options.Value.FileProvider ?? _environment.WebRootFileProvider;
                var fileInfo = fileProvider.GetFileInfo(_options.Value.FallbackFile);
                if (!fileInfo.Exists)
                {
                    LogFallbackFileWasntFoundOnDisk(_logger, _options.Value.FallbackFile);
                    return null;
                }
                using var stream = fileInfo.CreateReadStream();
                using var streamReader = new StreamReader(stream);
                var content = await streamReader.ReadToEndAsync();
                var context = new FallbackFileTransformContext(content, httpContext);
                var transformers = httpContext.RequestServices.GetServices<ICachedFallbackFileTransformer>();
                foreach (var transformer in transformers)
                {
                    await transformer.TransformAsync(context);
                }
                cacheEntry.SetSize(context.Content.Length * sizeof(char));
                cacheEntry.SetPriority(CacheItemPriority.High);
                return context.Content;
            });
            if (content == null)
            {
                httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                return "";
            }
            var context = new FallbackFileTransformContext(content, httpContext);
            foreach (var transformer in _transformers)
            {
                await transformer.TransformAsync(context);
            }
            if (_options.Value.FallbackFile.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
            {
                httpContext.Response.ContentType = _defaultContentType;
            }
            else
            {
                _options.Value.ContentTypeProvider.TryGetContentType(_options.Value.FallbackFile, out var contentType);
                httpContext.Response.ContentType = contentType ?? _defaultContentType;
            }

            return context.Content;
        }

        #region LogMessages

        [LoggerMessage(
            Level = LogLevel.Information,
            Message = "Fallback file wasn't found in cache. Creating new entry with key {key} for {fallbackFile}")]
        private static partial void LogFallbackFileWasntFoundInCache(ILogger logger, object key, string fallbackFile);

        [LoggerMessage(
            Level = LogLevel.Error,
            Message = "Fallback file wasn't found by file provider. Can't load and transform anything. {fallbackFile}")]
        private static partial void LogFallbackFileWasntFoundOnDisk(ILogger logger, string fallbackFile);
        #endregion
    }
}
