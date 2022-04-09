using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace IndexHtmlReWriter
{
    public class MemoryFileInfo : IFileInfo
    {
        private readonly byte[] _content;

        public MemoryFileInfo(
            byte[] content, string name, DateTimeOffset lastModified)
        {
            _content = content;
            Name = name;
            VirtualPath = "/" + Name;
            LastModified = lastModified;
        }

        public bool Exists => true;

        public bool IsDirectory => false;

        public DateTimeOffset LastModified { get; }

        public long Length => _content.Length;

        public string Name { get; }

        public string VirtualPath { get; }

        public string? PhysicalPath => null;

        public Stream CreateReadStream()
        {
            return new MemoryStream(_content, false);
        }
    }

}
