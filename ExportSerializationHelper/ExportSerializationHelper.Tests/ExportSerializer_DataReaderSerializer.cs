using ExportSerializationHelper.Tests.Data;
using System.IO;
using Xunit;

namespace ExportSerializationHelper.Tests;

public class ExportSerializer_DataReaderSerializer
{

    [Fact]
    public void DataReaderSerializer_Exports_Into_MiniExcel()
    {
        var config = new ExampleClassMap();
        var data = TestData.GenerateExampleClasses();
        using var stream = new MemoryStream();


        MiniExcelLibs.MiniExcel.SaveAs(stream, ExportSerializer.DataReaderSerializer(data, config));

        ResultData.VerifyExcel(stream);
    }
}
