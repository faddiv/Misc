using ExportSerializationHelper.Tests.Data;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ExportSerializationHelper.Tests
{
    public class SourceReaderTests
    {
        [Fact]
        public void Map_Extract_Property_Name_As_Field_Name_ByDefault()
        {
            var map = new SourceReader<ExampleClass>();
            map.Map(e => e.Id);
            map.Map(e => e.TextValue);
            map.Map(e => e.TimestampValue);
            map.Map(e => e.NullableDate);

            map.Members.Should().HaveCount(4);
            map.Members[0].Name.Should().Be(nameof(ExampleClass.Id));
            map.Members[1].Name.Should().Be(nameof(ExampleClass.TextValue));
            map.Members[2].Name.Should().Be(nameof(ExampleClass.TimestampValue));
            map.Members[3].Name.Should().Be(nameof(ExampleClass.NullableDate));
        }

        [Fact]
        public void Map_Extract_Property_Names_As_Field_Name_Chain_ByDefault()
        {
            var map = new SourceReader<ExampleClass2>();
            map.Map(e => e.ExampleRef!.Id);
            map.Map(e => e.ExampleRef!.TextValue);
            map.Map(e => e.ExampleRef!.TimestampValue);
            map.Map(e => e.ExampleRef!.NullableDate);

            map.Members.Should().HaveCount(4);
            map.Members[0].Name.Should().Be($"{nameof(ExampleClass2.ExampleRef)} {nameof(ExampleClass.Id)}");
            map.Members[1].Name.Should().Be($"{nameof(ExampleClass2.ExampleRef)} {nameof(ExampleClass.TextValue)}");
            map.Members[2].Name.Should().Be($"{nameof(ExampleClass2.ExampleRef)} {nameof(ExampleClass.TimestampValue)}");
            map.Members[3].Name.Should().Be($"{nameof(ExampleClass2.ExampleRef)} {nameof(ExampleClass.NullableDate)}");
        }
    }
}
