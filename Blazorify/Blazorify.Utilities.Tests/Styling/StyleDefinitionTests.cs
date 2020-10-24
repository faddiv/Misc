using FluentAssertions;
using System;
using Xunit;

namespace Blazorify.Utilities.Styling
{
    public class StyleDefinitionTests
    {
        private const string Width = "width";
        private const string Border = "border";
        private const string Height = "height";
        private const string Value1 = "100px";
        private const string Value2 = "200px";
        private const string Value3 = "1px";
        private const string Result = "width:100px;height:200px";

        private StyleBuilder styleBuilder = new StyleBuilder();
        private StyleDefinition CreateStyleBuilder()
        {
            return styleBuilder.Create();
        }

        [Fact]
        public void Add_string_string_bool_adds_stlyes_if_condition_is_true()
        {
            var builder = CreateStyleBuilder();

            var result = builder.Add(Width, Value1, true)
                .Add(Border, Value3, false)
                .Add(Height, Value2, true)
                .ToString();

            result.Should().Be(Result);
        }

        [Fact]
        public void Add_string_string_Func_bool_adds_stlyes_if_condition_is_true()
        {
            var builder = CreateStyleBuilder();

            var result = builder.Add(Width, Value1, () => true)
                .Add(Border, Value3, () => false)
                .Add(Height, Value2, () => true)
                .ToString();

            result.Should().Be(Result);
        }

        [Fact]
        public void Add_string_Func_string_bool_adds_stlyes_if_condition_is_true()
        {
            var builder = CreateStyleBuilder();

            var result = builder.Add(Width, () => Value1, true)
                .Add(Border, () => Value3, false)
                .Add(Height, () => Value2, true)
                .ToString();

            result.Should().Be(Result);
        }

        [Fact]
        public void Add_string_Func_string_Func_bool_adds_stlyes_if_condition_is_true()
        {
            var builder = CreateStyleBuilder();

            var result = builder.Add(Width, () => Value1, () => true)
                .Add(Border, () => Value3, () => false)
                .Add(Height, () => Value2, () => true)
                .ToString();

            result.Should().Be(Result);
        }

        [Fact]
        public void Add_StyleBuilder_adds_stlyes()
        {
            var builder = CreateStyleBuilder().Add(Width, Value1);
            var builderOther = CreateStyleBuilder()
                .Add(Height, Value2);

            var result = builder.Add(builderOther)
                .ToString();

            result.Should().Be(Result);
        }

        [Fact]
        public void Add_Dictionary_adds_stlye_key()
        {
            var builder = CreateStyleBuilder().Add(Width, Value1);
            var builderOther = CreateStyleBuilder()
                .Add(Height, Value2);

            var result = builder.Add(builderOther)
                .ToString();

            result.Should().Be(Result);
        }

        [Fact]
        public void AddMultiple_adds_string_string()
        {
            var builder = CreateStyleBuilder();

            var result = builder.AddMultiple(
                (Width, Value1),
                (Height, Value2))
                .ToString();

            result.Should().Be(Result);
        }

        [Fact]
        public void AddMultiple_adds_string_Func_string()
        {
            var builder = CreateStyleBuilder();

            var result = builder.AddMultiple(
                (Width, new Func<string>(() => Value1)),
                (Height, new Func<string>(() => Value2)))
                .ToString();

            result.Should().Be(Result);
        }

        [Fact]
        public void AddMultiple_adds_string_string_bool_if_true()
        {
            var builder = CreateStyleBuilder();

            var result = builder.AddMultiple(
                (Width, Value1, true),
                (Border, Value3, false),
                (Height, Value2, true))
                .ToString();

            result.Should().Be(Result);
        }

        [Fact]
        public void AddMultiple_adds_string_string_Func_bool_if_true()
        {
            var builder = CreateStyleBuilder();

            var result = builder.AddMultiple(
                (Width, Value1, new Func<bool>(() => true)),
                (Border, Value3, new Func<bool>(() => false)),
                (Height, Value2, new Func<bool>(() => true)))
                .ToString();

            result.Should().Be(Result);
        }

        [Fact]
        public void AddMultiple_adds_string_Func_string_bool_if_true()
        {
            var builder = CreateStyleBuilder();

            var result = builder.AddMultiple(
                (Width, new Func<string>(() => Value1), true),
                (Border, new Func<string>(() => Value3), false),
                (Height, new Func<string>(() => Value2), true))
                .ToString();

            result.Should().Be(Result);
        }

        [Fact]
        public void AddMultiple_adds_string_Func_string_Func_bool_if_true()
        {
            var builder = CreateStyleBuilder();

            var result = builder.AddMultiple(
                (Width, new Func<string>(() => Value1), new Func<bool>(() => true)),
                (Border, new Func<string>(() => Value3), new Func<bool>(() => false)),
                (Height, new Func<string>(() => Value2), new Func<bool>(() => true)))
                .ToString();

            result.Should().Be(Result);
        }

        [Fact]
        public void AddMultiple_adds_StyleBuilder()
        {
            var builder = CreateStyleBuilder().Add(Width, Value1);
            var builderOther = CreateStyleBuilder()
                .Add(Height, Value2);

            var result = builder.AddMultiple(builderOther)
                .ToString();

            result.Should().Be(Result);
        }
    }
}
