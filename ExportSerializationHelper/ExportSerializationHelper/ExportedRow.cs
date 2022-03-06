namespace ExportSerializationHelper
{
    public class ExportedRow
    {
        public ExportedRow(int index, object? original, object[] data, string[] header)
        {
            //System.Data.IDataReader
            IsHeader = index == 0;
            Index = index;
            Original = original;
            Data = data;
            Header = header;
        }

        public bool IsHeader { get; }

        public int Index { get; }

        public int DataIndex => Index - 1;

        public object? Original { get; }

        public object[] Data { get; }

        public string[] Header { get; }
    }
}
