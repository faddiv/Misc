namespace ExportSerializationHelper.Tests.Data
{
    public class ExampleClassMap : SourceReader<ExampleClass>
    {

        public ExampleClassMap()
        {
            Map(e => e.Id, cfg => cfg.Name("Identifier"));
            Map(e => e.TextValue, cfg => cfg.Name("Some Text"));
            Map(e => e.TimestampValue, cfg => cfg.Name("Timestamp"));
            Map(e => e.NullableDate, cfg => cfg.Name("Nullable"));
        }
    }
}
