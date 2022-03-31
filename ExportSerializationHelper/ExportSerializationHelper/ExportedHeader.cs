namespace ExportSerializationHelper;

public class ExportedHeader
{
    public ExportedHeader(int fieldIndex, string value)
    {
        FieldIndex = fieldIndex;
        Value = value;
    }

    public int FieldIndex { get; }

    public string Value { get; }
}
