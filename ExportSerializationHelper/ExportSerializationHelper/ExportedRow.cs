namespace ExportSerializationHelper
{
    public class ExportedRow
    {
        public ExportedRow(int index, object? original, object?[] data, string[] header)
        {
            Index = index;
            Original = original;
            Data = data;
            Header = header;
        }

        public int Index { get; }

        public object? Original { get; }

        public object?[] Data { get; }

        public string[] Header { get; }
    }
}
