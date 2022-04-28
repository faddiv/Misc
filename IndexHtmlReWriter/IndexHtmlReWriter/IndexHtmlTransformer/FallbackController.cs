using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace IndexHtmlReWriter.IndexHtmlTransformer
{
    public partial class FallbackController : Controller
    {
        private readonly IEnumerable<IPerRequestFallbackFileTransformer> _transformers;
        private readonly IOptions<FallbackOptions> _options;
        private readonly IWebHostEnvironment _environment;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<FallbackController> _logger;

        public FallbackController(
            IEnumerable<IPerRequestFallbackFileTransformer> transformers,
            IOptions<FallbackOptions> options,
            IWebHostEnvironment environment,
            IMemoryCache memoryCache,
            ILogger<FallbackController> logger)
        {
            _transformers = transformers;
            _options = options;
            _environment = environment;
            _memoryCache = memoryCache;
            _logger = logger;
        }
        public async Task<IActionResult> Index()
        {
            var content = await _memoryCache.GetOrCreateAsync(_options.Value.Key, GenerateContent);
            if (content == null)
            {
                return NotFound();
            }
            var context = new FallbackFileTransformContext(content, HttpContext);
            foreach (var transformer in _transformers)
            {
                await transformer.TransformAsync(context);
            }
            _options.Value.ContentTypeProvider.TryGetContentType(_options.Value.FallbackFile, out var contentType);
            return Content(context.Content, contentType ?? "text/html");
        }

        private async Task<string?> GenerateContent(ICacheEntry cacheEntry)
        {
            LogFallbackFileWasntFoundInCache(cacheEntry.Key, _options.Value.FallbackFile);
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
            var context = new FallbackFileTransformContext(content, HttpContext);
            var transformers = HttpContext.RequestServices.GetServices<ICachedFallbackFileTransformer>();
            foreach (var transformer in transformers)
            {
                await transformer.TransformAsync(context);
            }
            cacheEntry.SetSize(context.Content.Length * sizeof(char));
            cacheEntry.SetPriority(CacheItemPriority.High);
            return context.Content;
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
