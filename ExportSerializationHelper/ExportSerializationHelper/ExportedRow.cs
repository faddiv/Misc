namespace ExportSerializationHelper
{
    public class ExportedRow
    {
        public ExportedRow(int index, object original, object?[] data)
        {
            RowIndex = index;
            Original = original;
            Data = data;
        }

        public int RowIndex { get; }

        public object Original { get; }

        public object?[] Data { get; }
    }
}
