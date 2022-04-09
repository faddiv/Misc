using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace IndexHtmlReWriter
{
    public class SingleMemoryFileProvider : IFileProvider
    {
        private readonly MemoryFileInfo _memoryFileInfo;

        public SingleMemoryFileProvider(
            MemoryFileInfo memoryFileInfo)
        {
            _memoryFileInfo = memoryFileInfo;
        }
        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            throw new NotImplementedException();
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (!string.Equals(_memoryFileInfo.VirtualPath, subpath, StringComparison.Ordinal))
            {
                return new NotFoundFileInfo(subpath);
            }
            return _memoryFileInfo;
        }

        public IChangeToken Watch(string filter)
        {
            return NullChangeToken.Singleton;
        }
    }
}
