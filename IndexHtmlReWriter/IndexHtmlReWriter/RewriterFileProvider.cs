using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace IndexHtmlReWriter
{
    public class RewriterFileProvider : IFileProvider
    {
        private readonly IFileProvider _baseFileProvider;

        public RewriterFileProvider(IFileProvider baseFileProvider)
        {
            _baseFileProvider = baseFileProvider;
        }
        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return _baseFileProvider.GetDirectoryContents(subpath);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            return _baseFileProvider.GetFileInfo(subpath);
        }

        public IChangeToken Watch(string filter)
        {
            return _baseFileProvider.Watch(filter);
        }
    }
}
