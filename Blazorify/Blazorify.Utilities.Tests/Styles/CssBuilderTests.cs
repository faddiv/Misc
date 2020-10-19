using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace Blazorify.Utilities.Styles
{
    public class CssBuilderTests
    {
        [Fact]
        public void Create_with_no_parameter_generates_empty()
        {
            var result = CssBuilder.Create().ToString();

            result.Should().Be("");
        }

        [Fact]
        public void Create_with_parameter_generates_with_it()
        {
            var result = CssBuilder.Create("classname").ToString();

            result.Should().Be("classname");
        }

        [Fact]
        public void Add_adds_string_parameter()
        {
            var result = CssBuilder.Create("c1").Add("c2").ToString();

            result.Should().Be("c1 c2");
        }

        [Fact]
        public void Add_String_Condition_adds_string_parameter_if_condition_true()
        {
            var result = CssBuilder.Create("c1").Add("c2", false).Add("c3", true).ToString();

            result.Should().Be("c1 c3");
        }

        [Fact]
        public void Add_String_Predicate_adds_string_parameter_if_predicate_evaulates_true()
        {
            var result = CssBuilder.Create("c1").Add("c2", () => false).Add("c3", () => true).ToString();

            result.Should().Be("c1 c3");
        }

        [Fact]
        public void Add_Object_adds_Properties_as_css()
        {
            var result = CssBuilder.Create().Add(new { alfa = true, beta = true }).ToString();

            result.Should().Be("alfa beta");
        }

        [Fact]
        public void Add_Object_adds_Properties_only_true()
        {
            var result = CssBuilder.Create().Add(new { alfa = false, beta = true }).ToString();

            result.Should().Be("beta");
        }

        [Fact]
        public void Add_Object_replaces_underscores()
        {
            var result = CssBuilder.Create().Add(new { alfa_beta = true }).ToString();

            result.Should().Be("alfa-beta");
        }

        [Fact]
        public void Add_Tuple_adds_values_from_the_tuple_if_true()
        {
            var result = CssBuilder.Create().Add(("c1", true), ("c2", false), ("c3", true)).ToString();

            result.Should().Be("c1 c3");
        }

        [Fact]
        public void Add_Tuple_adds_values_from_the_tuple_if_evaulates_true()
        {
            var result = CssBuilder.Create().Add(
                ("c1", () => true),
                ("c2", () => false),
                ("c3", () => true)).ToString();

            result.Should().Be("c1 c3");
        }

        [Fact]
        public void Add_Attributes_adds_class_from_dictionary()
        {
            var attributes = new Dictionary<string, object>
            {
                {"class", "alfa" },
                {"other", 123 }
            };
            var result = CssBuilder.Create().Add(attributes).ToString();

            result.Should().Be("alfa");
        }

        [Fact]
        public void Create_accepts_multiple_different_parameters()
        {
            var attributes = new Dictionary<string, object>
            {
                {"class", "c5" },
                {"other", 123 }
            };

            var result = CssBuilder.Create(
                "c1",
                new { c2 = true },
                ("c3", true),
                ("c4", new Func<bool>(() => true)),
                attributes).ToString();

            result.Should().Be("c1 c2 c3 c4 c5");
        }

    }
}
