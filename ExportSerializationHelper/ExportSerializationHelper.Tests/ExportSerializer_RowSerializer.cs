using CsvHelper;
using CsvHelper.Configuration;
using ExportSerializationHelper.Tests.Data;
using FluentAssertions;
using Snapshooter.Xunit;
using System.Globalization;
using System.IO;
using System.Linq;
using Xunit;

namespace ExportSerializationHelper.Tests;

public class ExportSerializer_RowSerializer
{
    [Fact]
    public void RowSerializer_Exports_Into_Csv()
    {
        var config = new ExampleClassMap();
        var data = TestData.GenerateExampleClasses();
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false
        });

        foreach (var header in config.GetHeader())
        {
            csv.WriteField(header);
        }
        csv.NextRecord();

        foreach (var item in ExportSerializer.RowSerializer(data, config))
        {
            foreach (var field in item.Data)
            {
                csv.WriteField(field);
            }
            csv.NextRecord();
        }
        csv.Flush();
        var result = writer.Encoding.GetString(stream.ToArray());

        result.Should().MatchSnapshot();
    }

    [Fact]
    public void RowSerializer_Exports_Into_ClosedXml()
    {
        var config = new ExampleClassMap();
        var data = TestData.GenerateExampleClasses();
        using var stream = new MemoryStream();
        var workbook = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.AddWorksheet("Sheet1");
        worksheet.Cell(1, 1).InsertData(new[] { config.GetHeader() });
        worksheet.Cell(2, 1).InsertData(ExportSerializer.RowSerializer(data, config).Select(e => e.Data));

        ResultData.TextExcel(worksheet);
    }

}
