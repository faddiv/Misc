using ClosedXML.Excel;
using FluentAssertions;
using Snapshooter.Xunit;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace ExportSerializationHelper.Tests.Data
{
    public class ResultData
    {
        public static void VerifyExcel(
            IXLWorksheet worksheet, [CallerMemberName] string? caller = null)
        {
            //worksheet.Workbook.SaveAs(caller + ".xlsx");
            worksheet.SetAutoFilter(false);
            var rangeUsed = worksheet.RangeUsed();
            rangeUsed.Should().NotBeNull();
            var table = rangeUsed.AsTable();
            table.Theme = XLTableTheme.TableStyleLight6;
            var result = table.AsNativeDataTable();
            result.Should().MatchSnapshot();
            
        }

        internal static void VerifyExcel(MemoryStream stream,
            [CallerMemberName] string? caller = null)
        {
            stream.Seek(0, SeekOrigin.Begin);
            //File.WriteAllBytes(caller + ".xlsx", stream.ToArray());
            var book = new ClosedXML.Excel.XLWorkbook(stream);
            VerifyExcel(book.Worksheet(1), caller);
        }
    }
}
