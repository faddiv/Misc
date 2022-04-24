using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace IndexHtmlReWriter.IndexHtmlTransformer
{
    public partial class FallbackController : Controller
    {
        private readonly IEnumerable<ICachedFallbackFileTransformer> _cachedTransformers;
        private readonly IEnumerable<IPerRequestFallbackFileTransformer> _perRequestTransformers;
        private readonly IOptions<FallbackOptions> _options;
        private readonly IWebHostEnvironment _environment;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<FallbackController> _logger;

        public FallbackController(
            IEnumerable<ICachedFallbackFileTransformer> cachedTransformers,
            IEnumerable<IPerRequestFallbackFileTransformer> perRequestTransformers,
            IOptions<FallbackOptions> options,
            IWebHostEnvironment environment,
            IMemoryCache memoryCache,
            ILogger<FallbackController> logger)
        {
            _cachedTransformers = cachedTransformers;
            _perRequestTransformers = perRequestTransformers;
            _options = options;
            _environment = environment;
            _memoryCache = memoryCache;
            _logger = logger;
        }
        public async Task<IActionResult> Index()
        {
            var content = await _memoryCache.GetOrCreateAsync(_options.Value.Key, async (key) =>
            {
                LogFallbackFileWasntFoundInCache(key.Key, _options.Value.FallbackFile);
                var fileProvider = _options.Value.FileProvider ?? _environment.WebRootFileProvider;
                var fileInfo = fileProvider.GetFileInfo(_options.Value.FallbackFile);
                if (!fileInfo.Exists)
                {
                    LogFallbackFileWasntFoundOnDisk(_options.Value.FallbackFile);
                    return null;
                }
                using var stream = fileInfo.CreateReadStream();
                using var streamReader = new StreamReader(stream);
                var content = await streamReader.ReadToEndAsync();
                foreach (var transformer in _cachedTransformers)
                {
                    content = await transformer.TransformAsync(content, HttpContext);
                }
                return content;
            });
            if (content == null)
            {
                return NotFound();
            }
            foreach (var transformer in _perRequestTransformers)
            {
                content = await transformer.TransformAsync(content, HttpContext);
            }
            _options.Value.ContentTypeProvider.TryGetContentType(_options.Value.FallbackFile, out var contentType);
            return Content(content, contentType ?? "text/html");
        }

        public IActionResult Authenticated()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                return Content("document.body.setAttribute(\"authenticated\", \"\");", "text/javascript");
            }
            return Content("/* unauthenticated */", "text/javascript");
        }

        #region LogMessages

        [LoggerMessage(
            Level = LogLevel.Information,
            Message = "Fallback file wasn't found in cache. Creating new entry with key {key} for {fallbackFile}")]
        private partial void LogFallbackFileWasntFoundInCache(object key, string fallbackFile);

        [LoggerMessage(
    Level = LogLevel.Error,
    Message = "Fallback file wasn't found by file provider. Can't load and transform anything. {fallbackFile}")]
        private partial void LogFallbackFileWasntFoundOnDisk(string fallbackFile);
        #endregion
    }
}
