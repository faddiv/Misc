using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System.Text;
using System.Text.RegularExpressions;

namespace IndexHtmlReWriter
{
    public class RewriterFileProvider : IFileProvider
    {
        private readonly IFileProvider _baseFileProvider;
        private readonly Func<string, string> _rewriterCallback;
        private readonly Encoding _encoding;

        public RewriterFileProvider(
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

        private class RewriterFileInfo : IFileInfo
        {
            private readonly RewriterFileProvider _parent;
            private readonly IFileInfo _baseFileInfo;
            private byte[]? _rewrittedData;

            public RewriterFileInfo(
                IFileInfo baseFileInfo,
                RewriterFileProvider parent)
            {
                _baseFileInfo = baseFileInfo;
                _parent = parent;
            }

            public bool Exists => _baseFileInfo.Exists;

            public bool IsDirectory => _baseFileInfo.IsDirectory;

            public DateTimeOffset LastModified => _baseFileInfo.LastModified;

            public long Length => GetRewritedData().Length;

            public string Name => _baseFileInfo.Name;

            public string? PhysicalPath => null;

            public Stream CreateReadStream()
            {
                return new MemoryStream(GetRewritedData());
            }

            private byte[] GetRewritedData()
            {
                if (_rewrittedData is not null)
                    return _rewrittedData;
                using var source = _baseFileInfo.CreateReadStream();
                using var reader = new System.IO.StreamReader(source);
                var text = reader.ReadToEnd();
                var newText = _parent._rewriterCallback(text);
                _rewrittedData = _parent._encoding.GetBytes(newText);
                return _rewrittedData;
            }
        }
    }

}
