using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System.Text;
using System.Text.RegularExpressions;

namespace IndexHtmlReWriter
{
    public partial class MemoryFileProvider : IFileProvider
    {
        private readonly MemoryFileInfo _memoryFileInfo;

        public MemoryFileProvider(
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
            if (subpath != _memoryFileInfo.Name)
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
