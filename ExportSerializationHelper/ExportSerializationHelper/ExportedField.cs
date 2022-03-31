namespace ExportSerializationHelper;

public class ExportedField
{
    public ExportedField(int rowIndex, int fieldIndex, object original, object? value, bool nextRow)
    {
        RowIndex = rowIndex;
        FieldIndex = fieldIndex;
        Original = original;
        Value = value;
        NextRow = nextRow;
    }
    public int RowIndex { get; }

    public int FieldIndex { get; }

    public object Original { get; }

    public object? Value { get; }

    public bool NextRow { get; }
}
