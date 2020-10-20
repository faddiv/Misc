using Blazorify.Utilities.Styling;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace Blazorify.Utilities.Styles
{
    public class CssBuilderTests
    {
        private static CssBuilder CreateCssBuilder()
        {
            return CssBuilder.Create();
        }

        [Fact]
        public void Create_with_no_parameter_generates_empty()
        {
            var result = CreateCssBuilder().ToString();

            result.Should().Be("");
        }

        [Fact]
        public void Add_adds_string_parameter()
        {
            var result = CreateCssBuilder().Add("c1").Add("c2").ToString();

            result.Should().Be("c1 c2");
        }

        [Fact]
        public void Add_String_Condition_adds_string_parameter_if_condition_true()
        {
            var result = CreateCssBuilder().Add("c1").Add("c2", false).Add("c3", true).ToString();

            result.Should().Be("c1 c3");
        }

        [Fact]
        public void Add_String_Predicate_adds_string_parameter_if_predicate_evaulates_true()
        {
            var result = CreateCssBuilder().Add("c1").Add("c2", () => false).Add("c3", () => true).ToString();

            result.Should().Be("c1 c3");
        }

        [Fact]
        public void Add_Object_adds_Properties_as_css()
        {
            var result = CreateCssBuilder().Add(new { alfa = true, beta = true }).ToString();

            result.Should().Be("alfa beta");
        }

        [Fact]
        public void Add_Object_adds_Properties_only_true()
        {
            var result = CreateCssBuilder().Add(new { alfa = false, beta = true }).ToString();

            result.Should().Be("beta");
        }

        [Fact]
        public void Add_Object_replaces_underscores()
        {
            var result = CreateCssBuilder().Add(new { alfa_beta = true }).ToString();

            result.Should().Be("alfa-beta");
        }

        [Fact]
        public void Add_Tuple_adds_values_from_the_tuple_if_true()
        {
            var result = CreateCssBuilder().Add(("c1", true), ("c2", false), ("c3", true)).ToString();

            result.Should().Be("c1 c3");
        }

        [Fact]
        public void Add_Tuple_adds_values_from_the_tuple_if_evaulates_true()
        {
            var result = CreateCssBuilder().Add(
                ("c1", () => true),
                ("c2", () => false),
                ("c3", () => true)).ToString();

            result.Should().Be("c1 c3");
        }

        [Fact]
        public void Add_Enumerable_string_adds_value_enumerable()
        {
            var result = CreateCssBuilder().Add(
                new[] { "c1", "c2" }).ToString();

            result.Should().Be("c1 c2");
        }

        [Fact]
        public void Add_CssBuilder_adds_from_another_builder()
        {
            var original = CreateCssBuilder().Add("c2").Add("c3");
            var result = CreateCssBuilder().Add("c1")
                .Add(original).ToString();

            result.Should().Be("c1 c2 c3");
        }

        [Fact]
        public void Add_Attributes_adds_class_from_dictionary()
        {
            var attributes = new Dictionary<string, object>
            {
                {"class", "alfa" },
                {"other", 123 }
            };
            var result = CreateCssBuilder().Add(attributes).ToString();

            result.Should().Be("alfa");
        }

        [Fact]
        public void Add_Enum_adds_enum_by_naming_convention()
        {
            var value = Dummy.NameName_name;
            var result = CreateCssBuilder().Add(value).ToString();

            result.Should().Be("name-name_name");
        }

        [Fact]
        public void AddMultiple_adds_input_to_the_list()
        {
            var cssBuilder = CreateCssBuilder().Add("c1");

            var result = cssBuilder.AddMultiple("c2").ToString();

            result.Should().Be("c1 c2");
        }

        [Fact]
        public void AddMultiple_accepts_multiple_different_parameters()
        {
            var attributes = new Dictionary<string, object>
            {
                {"class", "c7" },
                {"other", 123 }
            };
            var other = CreateCssBuilder().Add("c8").Add("c9");

            var result = CreateCssBuilder().AddMultiple(
                "c1",
                new { c2 = true },
                ("c3", true),
                ("c4", new Func<bool>(() => true)),
                new[] { "c5", "c6" },
                attributes,
                other,
                Dummy.c10
                ).ToString();

            result.Should().Be("c1 c2 c3 c4 c5 c6 c7 c8 c9 c10");
        }

        public enum Dummy
        {
            NameName_name,
            c10
        }

    }
}
