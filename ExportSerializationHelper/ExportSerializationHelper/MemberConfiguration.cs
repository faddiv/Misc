using System;

namespace ExportSerializationHelper
{
    public class MemberConfiguration
    {
        public MemberConfiguration(string name)
        {
            Name = name;
        }

        public string Name { get; internal set; }

        public Func<object, object?> Getter { get; internal set; } = (object value) => value;
    }
}
