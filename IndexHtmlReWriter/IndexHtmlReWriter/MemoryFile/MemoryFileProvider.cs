using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System.Text;
using System.Text.RegularExpressions;

namespace IndexHtmlReWriter
{
    public class MemoryFileProvider : IFileProvider
    {
        private readonly IFileProvider _baseFileProvider;
        private readonly Func<string, string> _rewriterCallback;
        private readonly Encoding _encoding;

        public MemoryFileProvider(
            IFileProvider baseFileProvider,
            Func<string, string> rewriterCallback,
            Encoding? encoding = null)
        {
            _baseFileProvider = baseFileProvider ?? throw new ArgumentNullException(nameof(baseFileProvider));
            _rewriterCallback = rewriterCallback ?? throw new ArgumentNullException(nameof(rewriterCallback));
            _encoding = encoding ?? Encoding.UTF8;
        }
        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return _baseFileProvider.GetDirectoryContents(subpath);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            return new RewriterFileInfo(_baseFileProvider.GetFileInfo(subpath), this);
        }

        public IChangeToken Watch(string filter)
        {
            return _baseFileProvider.Watch(filter);
        }

    }
}
