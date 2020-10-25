using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace Blazorify.Utilities.Styling
{
    public class CssDefinitionTests
    {
        private CssBuilder cssBuilder = new CssBuilder(new CssBuilderOptions());

        private CssDefinition CreateCssDefinition(CssBuilderOptions options = null)
        {
            return cssBuilder.Create(options);
        }

        [Fact]
        public void Create_with_no_parameter_generates_empty()
        {
            var result = CreateCssDefinition().ToString();

            result.Should().Be("");
        }

        [Fact]
        public void Add_adds_string_parameter()
        {
            var result = CreateCssDefinition().Add("c1").Add("c2").ToString();

            result.Should().Be("c1 c2");
        }

        [Fact]
        public void Add_String_Condition_adds_string_parameter_if_condition_true()
        {
            var result = CreateCssDefinition().Add("c1").Add("c2", false).Add("c3", true).ToString();

            result.Should().Be("c1 c3");
        }

        [Fact]
        public void Add_String_Predicate_adds_string_parameter_if_predicate_evaulates_true()
        {
            var result = CreateCssDefinition().Add("c1").Add("c2", () => false).Add("c3", () => true).ToString();

            result.Should().Be("c1 c3");
        }

        [Fact]
        public void Add_Object_adds_Properties_as_css()
        {
            var result = CreateCssDefinition().Add(new { alfa = true, beta = true }).ToString();

            result.Should().Be("alfa beta");
        }

        [Fact]
        public void Add_Object_adds_Properties_only_true()
        {
            var result = CreateCssDefinition().Add(new { alfa = false, beta = true }).ToString();

            result.Should().Be("beta");
        }

        [Fact]
        public void Add_Object_replaces_underscores()
        {
            var result = CreateCssDefinition().Add(new { alfa_beta = true }).ToString();

            result.Should().Be("alfa-beta");
        }

        [Fact]
        public void Add_Tuple_adds_values_from_the_tuple_if_true()
        {
            var result = CreateCssDefinition().Add(("c1", true), ("c2", false), ("c3", true)).ToString();

            result.Should().Be("c1 c3");
        }

        [Fact]
        public void Add_Tuple_adds_values_from_the_tuple_if_evaulates_true()
        {
            var result = CreateCssDefinition().Add(
                ("c1", () => true),
                ("c2", () => false),
                ("c3", () => true)).ToString();

            result.Should().Be("c1 c3");
        }

        [Fact]
        public void Add_Enumerable_string_adds_value_enumerable()
        {
            var result = CreateCssDefinition().Add(
                new[] { "c1", "c2" }).ToString();

            result.Should().Be("c1 c2");
        }

        [Fact]
        public void Add_CssBuilder_adds_from_another_builder()
        {
            var original = CreateCssDefinition().Add("c2").Add("c3");
            var result = CreateCssDefinition().Add("c1")
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
            var result = CreateCssDefinition().Add(attributes).ToString();

            result.Should().Be("alfa");
        }

        [Fact]
        public void Add_Enum_adds_enum_by_naming_convention()
        {
            var value = Dummy.NameName_name;
            var result = CreateCssDefinition().Add(value).ToString();

            result.Should().Be("name-name-name");
        }

        [Fact]
        public void AddMultiple_adds_input_to_the_list()
        {
            var cssBuilder = CreateCssDefinition().Add("c1");

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
            var other = CreateCssDefinition().Add("c8").Add("c9");

            var result = CreateCssDefinition().AddMultiple(
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

        [Fact]
        public void Enum_caching_works_correctly()
        {
            var result1 = CreateCssDefinition().AddMultiple(Dummy.NameName_name, Dummy.c10, Dummy2.SomeOther, Dummy2.c20).ToString();
            var result2 = CreateCssDefinition().AddMultiple(Dummy.NameName_name, Dummy.c10, Dummy2.SomeOther, Dummy2.c20).ToString();

            result1.Should().Be("name-name-name c10 some-other c20");
            result1.Should().Be(result2);
        }

        public enum Dummy
        {
            NameName_name = 1,
            c10 = 2
        }

        public enum Dummy2
        {
            SomeOther = 1,
            c20 = 2
        }
    }
}
