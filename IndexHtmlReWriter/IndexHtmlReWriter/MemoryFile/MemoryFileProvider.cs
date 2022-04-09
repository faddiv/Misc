using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System.Collections.Concurrent;

namespace IndexHtmlReWriter
{
    public class MemoryFileProvider : IFileProvider
    {
        private readonly ConcurrentDictionary<string, MemoryFileInfo> _files;

        public MemoryFileProvider()
        {
            _files = new ConcurrentDictionary<string, MemoryFileInfo>();
        }

        public MemoryFileInfo SetFile(string virtualPath, byte[] contents, DateTimeOffset lastModified)
        {
            return _files.AddOrUpdate(virtualPath,
                (key, arg) =>
            {
                return new MemoryFileInfo(arg.contents, key, arg.lastModified);
            }, (key, _, arg) =>
            {
                return new MemoryFileInfo(arg.contents, key, arg.lastModified);
            }, (contents, lastModified));
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            List<MemoryFileInfo> localFiles = new();
            foreach (var item in _files)
            {
                if (item.Key.StartsWith(subpath))
                {
                    localFiles.Add(item.Value);
                }
            }
            return new MemoryDirectoryContents(localFiles);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (_files.TryGetValue(subpath, out var file))
            {
                return file;
            }
            return new NotFoundFileInfo(subpath);
        }

        public IChangeToken Watch(string filter)
        {
            return NullChangeToken.Singleton;
        }
    }
}
