using Microsoft.Extensions.FileProviders;
using System.Collections;

namespace IndexHtmlReWriter
{
    public class MemoryDirectoryContents : IDirectoryContents
    {
        private readonly List<MemoryFileInfo> _files;

        public MemoryDirectoryContents(List<MemoryFileInfo> files)
        {
            _files = files;
        }
        public bool Exists => _files.Count > 0;

        public IEnumerator<IFileInfo> GetEnumerator()
        {
            return _files.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
