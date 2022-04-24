using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

namespace IndexHtmlReWriter.IndexHtmlTransformer
{
    public class FallbackOptions
    {
        public FallbackOptions()
        {
            ContentTypeProvider = new FileExtensionContentTypeProvider();
        }
        public string Key { get; set; } = "FallbackFile";

        public string FallbackFile { get; set; } = "index.html";

        public IFileProvider? FileProvider { get; set; }

        public IContentTypeProvider ContentTypeProvider { get; set; }
    }
}
