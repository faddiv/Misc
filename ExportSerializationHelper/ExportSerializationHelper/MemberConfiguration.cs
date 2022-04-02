using System;

namespace ExportSerializationHelper
{
    public class MemberConfiguration
    {
        private string? _name;
        public MemberConfiguration(Type valueType)
        {
            ValueType = valueType;
        }

        public string Name
        {
            get => _name ?? "";
            internal set => _name = value;
        }

        public Type ValueType { get; }

        public Func<object, object?> Getter { get; internal set; } = (object value) => value;

        internal bool IsNameUnset()
        {
            return _name == null;
        }
    }
}
