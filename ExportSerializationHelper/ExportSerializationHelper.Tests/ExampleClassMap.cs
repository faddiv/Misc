namespace ExportSerializationHelper.Tests
{
    public class ExampleClassMap : SourceReader<ExampleClass>
    {

        public ExampleClassMap()
        {
            MapDirect(e => e.Id, cfg => cfg.Name("Identifier"));
            MapDirect(e => e.TextValue, cfg => cfg.Name("Some Text"));
            MapDirect(e => e.TimestampValue, cfg => cfg.Name("Timestamp"));
            MapDirect(e => e.NullableDate, cfg => cfg.Name("Nullable"));
        }
    }
}
