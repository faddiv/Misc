using ClosedXML.Excel;
using FluentAssertions;
using Snapshooter.Xunit;
using System.Runtime.CompilerServices;

namespace ExportSerializationHelper.Tests
{
    public class ResultData
    {
        public static void TextExcel(
            IXLWorksheet worksheet, [CallerMemberName] string? caller = null)
        {
            var rangeUsed = worksheet.RangeUsed();
            rangeUsed.Should().NotBeNull();
            var table = rangeUsed.CreateTable("Table1");
            table.Theme = XLTableTheme.TableStyleLight6;
            var result = table.AsNativeDataTable();
            result.Should().MatchSnapshot();
            //worksheet.Workbook.SaveAs(caller + ".xlsx");
        }
    }
}
