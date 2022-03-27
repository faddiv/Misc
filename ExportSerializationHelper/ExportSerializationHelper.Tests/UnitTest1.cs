using Bogus;
using CsvHelper;
using CsvHelper.Configuration;
using DocumentFormat.OpenXml.Packaging;
using FluentAssertions;
using Snapshooter.Xunit;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using Xunit;

namespace ExportSerializationHelper.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var today = DateTime.Parse("2022-03-06");
            var config = new ExampleClassMap();
            var faker = new Faker<ExampleClass>()
                .UseSeed(1234)
                .RuleFor(e => e.Id, f => f.Random.Number(1000))
                .RuleFor(e => e.TextValue, f => f.Random.AlphaNumeric(10))
                .RuleFor(e => e.TimestampValue, f => f.Date.Recent(refDate: today))
                .RuleFor(e => e.NullableDate, f => f.Random.Bool() ? f.Date.Recent(refDate: today) : null);
            var data = faker.Generate(10);
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

            foreach (var item in ExportSerializer.Serialize(data, config))
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
        public void Test2()
        {
            var ctl = new SamplesController();
            var mem = ctl.CreateExcelFile();
            File.WriteAllBytes("excel.xlsx", mem.ToArray());
        }

    }
}
