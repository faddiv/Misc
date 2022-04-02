using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using ExportSerializationHelper.Tests.Data;
using FluentAssertions;
using Snapshooter.Xunit;
using System;
using System.Globalization;
using System.IO;
using Xunit;

namespace ExportSerializationHelper.Tests;

public class ExportSerializer_FieldSerializer
{
    [Fact]
    public void FieldSerializer_Exports_Into_Csv()
    {
        var today = DateTime.Parse("2022-03-06");
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

        foreach (var item in ExportSerializer.FieldSerializer(data, config))
        {
            csv.WriteField(item.Value);
            if (item.NextRow)
            {
                csv.NextRecord();
            }
        }
        csv.Flush();
        var result = writer.Encoding.GetString(stream.ToArray());

        result.Should().MatchSnapshot();
    }

    [Fact]
    public void FieldSerializer_Exports_Into_ClosedXml()
    {
        var config = new ExampleClassMap();
        var data = TestData.GenerateExampleClasses();
        var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("Sheet1");
        foreach (var item in config.HeaderSerializer())
        {
            worksheet.Cell(1, 1 + item.FieldIndex).SetValue(item.Value);
        }

        foreach (var item in ExportSerializer.FieldSerializer(data, config))
        {
            worksheet.Cell(2 + item.RowIndex, 1 + item.FieldIndex).SetValue(item.Value);
        }

        ResultData.TextExcel(worksheet);
    }
}
