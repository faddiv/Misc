using System;
using System.Collections.Generic;

namespace ExportSerializationHelper
{
    public class ExportSerializer
    {
        public static IEnumerable<ExportedRow> Serialize<TSourceReader, TModel>(IEnumerable<TModel> source, TSourceReader sourceReader)
            where TSourceReader : SourceReader
        {
            sourceReader.EnsureInitialized();
            int index = 0;
            foreach (var item in source)
            {
                if (item == null)
                    throw new ArgumentException("The item of the datasource can't be null");
                var data = sourceReader.Read(item);
                yield return new ExportedRow(index, item, data, sourceReader.GetHeader());
                index++;
            }
        }
    }
}
