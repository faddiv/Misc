using Microsoft.Extensions.FileProviders;
using System.Text;

namespace IndexHtmlReWriter
{
    public class MemoryFileInfo : IFileInfo
    {
        private readonly MemoryFileProvider _parent;
        private readonly IFileInfo _baseFileInfo;
        private byte[]? _rewrittedData;

        public MemoryFileInfo(
            IFileInfo baseFileInfo,
            MemoryFileProvider parent)
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
