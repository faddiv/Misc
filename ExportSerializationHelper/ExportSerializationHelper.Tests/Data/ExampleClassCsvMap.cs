using CsvHelper.Configuration;

namespace ExportSerializationHelper.Tests.Data
{
    public class ExampleClassCsvMap : ClassMap<ExampleClass>
    {
        public ExampleClassCsvMap()
        {
            Map(e => e.Id).Name("Identifier");
            Map(e => e.TextValue).Name("Some Text");
            Map(e => e.TimestampValue).Name("Timestamp");
            Map(e => e.NullableDate).Name("Nullable");
        }
    }
}
