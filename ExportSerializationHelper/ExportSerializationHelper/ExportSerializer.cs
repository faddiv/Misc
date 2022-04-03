using System;
using System.Collections.Generic;
using System.Data;

namespace ExportSerializationHelper
{
    public static class ExportSerializer
    {
        public static IEnumerable<ExportedRow> RowSerializer<TSourceReader, TModel>(IEnumerable<TModel> source, TSourceReader sourceReader)
            where TSourceReader : SourceReader
        {
            int rowIndex = 0;
            foreach (var item in source)
            {
                if (item == null)
                    throw new ArgumentException("The item of the datasource can't be null");

                object?[] data = new object?[sourceReader.CountFields];
                for (int fieldIndex = 0; fieldIndex < sourceReader.CountFields; fieldIndex++)
                {
                    data[fieldIndex] = sourceReader.GetValue(item, fieldIndex);
                }
                yield return new ExportedRow(rowIndex, item, data);
                rowIndex++;
            }
        }

        public static IEnumerable<ExportedField> FieldSerializer<TSourceReader, TModel>(IEnumerable<TModel> source, TSourceReader sourceReader)
            where TSourceReader : SourceReader
        {
            int rowIndex = 0;
            int lastIndex = sourceReader.CountFields - 1;
            foreach (var item in source)
            {
                if (item == null)
                    throw new ArgumentException("The item of the datasource can't be null");

                for (int fieldIndex = 0; fieldIndex < sourceReader.CountFields; fieldIndex++)
                {
                    var value = sourceReader.GetValue(item, fieldIndex);
                    yield return new ExportedField(rowIndex, fieldIndex, item, value, fieldIndex == lastIndex);
                }
                rowIndex++;
            }
        }

        public static IDataReader DataReaderSerializer<TSourceReader, TModel>(this IEnumerable<TModel> source, TSourceReader reader)
            where TSourceReader : SourceReader
        {
            return new ExportDataReader(reader, source);
        }
    }
}
