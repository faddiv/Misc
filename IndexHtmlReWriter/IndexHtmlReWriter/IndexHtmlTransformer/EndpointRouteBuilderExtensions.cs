using System.Text;

namespace IndexHtmlReWriter.IndexHtmlTransformer
{
    public static class EndpointRouteBuilderExtensions
    {
        public static IEndpointConventionBuilder MapFallbackToMemoryFile(
               this IEndpointRouteBuilder endpoints,
               string name,
               string virtualPath,
               StaticFileOptions? options = null)
        {
            return endpoints.MapFallbackToMemoryFile(name, content =>
            {
                return content.Replace("<base href=\"./\"/>", $"<base href=\"{virtualPath}\"/>");
            }, options);
        }
        public static IEndpointConventionBuilder MapFallbackToMemoryFile(
            this IEndpointRouteBuilder endpoints,
            string name,
            Func<string, string> transform,
            StaticFileOptions? options = null)
        {
            var env = options?.FileProvider
                ?? (endpoints as WebApplication)?.Environment.WebRootFileProvider
                ?? endpoints.ServiceProvider.GetRequiredService<IWebHostEnvironment>().WebRootFileProvider;
            var fileInfo = env.GetFileInfo(name);
            using var stream = fileInfo.CreateReadStream();
            using var reader = new StreamReader(stream, true);
            var content = reader.ReadToEnd();
            content = transform(content);
            var data = Encoding.UTF8.GetBytes(content);
            var indexProvider = new SingleMemoryFileProvider(new MemoryFileInfo(data, name, fileInfo.LastModified));
            var options2 = new StaticFileOptions { FileProvider = indexProvider };
            if (options != null)
            {
                options2.ContentTypeProvider = options.ContentTypeProvider;
                options2.DefaultContentType = options.DefaultContentType;
                options2.HttpsCompression = options.HttpsCompression;
                options2.OnPrepareResponse = options.OnPrepareResponse;
                options2.RedirectToAppendTrailingSlash = options.RedirectToAppendTrailingSlash;
                options2.RequestPath = options.RequestPath;
                options2.ServeUnknownFileTypes = options.ServeUnknownFileTypes;
            }
            options2.FileProvider = indexProvider;
            return endpoints.MapFallbackToFile("index.html", options2);
        }
    }
}
