using System;

namespace ExportSerializationHelper.Tests.Data
{
    public class ExampleClass
    {
        public int Id { get; set; }
        public string? TextValue { get; set; }
        public DateTime TimestampValue { get; set; }
        public DateTime? NullableDate { get; set; }
    }
}
