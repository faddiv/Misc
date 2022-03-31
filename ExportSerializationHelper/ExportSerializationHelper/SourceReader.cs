using System.Collections.Generic;
using System.Linq;

namespace ExportSerializationHelper
{
    public class SourceReader
    {
        private bool _initialized;
        private readonly object _lock;
        private readonly List<MemberConfiguration> _members;
        private string[]? _header;

        public SourceReader()
        {
            _members = new List<MemberConfiguration>();
            _lock = new object();
        }

        public int CountFields => _members.Count;

        protected internal void AddMember(MemberConfiguration configuration)
        {
            _members.Add(configuration);
        }

        public string[] GetHeader()
        {
            if (_header == null)
            {
                _header = _members.Select(e => e.Name).ToArray();
            }
            return _header;
        }

        public IEnumerable<ExportedHeader> HeaderSerializer()
        {
            var header = GetHeader();
            for (int fieldIndex = 0; fieldIndex < CountFields; fieldIndex++)
            {
                yield return new ExportedHeader(fieldIndex, header[fieldIndex]);
            }
        }

        internal object? GetValue(object source, int index)
        {
            return _members[index].Getter(source);
        }

        public bool Initialized => _initialized;
    }
}
