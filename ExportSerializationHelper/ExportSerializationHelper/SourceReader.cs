using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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

        public void EnsureInitialized()
        {
            if (Volatile.Read(ref _initialized))
            {
                return;
            }
            lock (_lock)
            {
                if (!Volatile.Read(ref _initialized))
                {
                    InitializeCore();
                    Volatile.Write(ref _initialized, true);
                }
            }
        }

        private void InitializeCore()
        {
        }

        protected internal void AddMember(MemberConfiguration configuration)
        {
            _members.Add(configuration);
        }

        public string[] GetHeader()
        {
            EnsureInitialized();
            if(_header == null)
            {
                _header = _members.Select(e => e.Name).ToArray();
            }
            return _header;
        }

        internal object?[] Read(object item)
        {
            var row = new object?[CountFields];
            for (int i = 0; i < CountFields; i++)
            {
                row[i] = _members[i].Getter(item);
            }
            return row;
        }
        public bool Initialized => _initialized;
    }
}
