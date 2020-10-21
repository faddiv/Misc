using Blazorify.Utilities.Styling;
using FluentAssertions;
using System.Reflection;
using Xunit;

namespace Blazorify.Utilities.Styles
{
    public class DefaultCssBuilderNamingConventionPropertyTests
    {
        public DefaultCssBuilderNamingConvention instance = new DefaultCssBuilderNamingConvention();

        public PropertyInfo PascalCase = typeof(Dummy).GetProperty(nameof(Dummy.PascalCase));

        public PropertyInfo PascalCase_WithUnderScore = typeof(Dummy).GetProperty(nameof(Dummy.PascalCase_WithUnderScore));

        [Fact]
        public void DefaultValue_is_correct()
        {
            instance.PropertyMode.Should().Be(CssBuilderNamingMode.KebabCase);
            instance.PropertyUnderscoreToHyphen.Should().BeTrue();
        }

        [Theory]
        [InlineData(false, "PascalCase_WithUnderScore")]
        [InlineData(true, "PascalCase-WithUnderScore")]
        public void ToCssClassName_uses_PropertyUnderscoreToHyphen_value(
            bool propertyUnderscoreToHyphen, string expected)
        {
            instance.PropertyMode = CssBuilderNamingMode.None;
            instance.PropertyUnderscoreToHyphen = propertyUnderscoreToHyphen;

            var result = instance.ToCssClassName(PascalCase_WithUnderScore);

            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(CssBuilderNamingMode.None, "PascalCase")]
        [InlineData(CssBuilderNamingMode.KebabCase, "pascal-case")]
        public void ToCssClassName_uses_PropertyMode_value(
            CssBuilderNamingMode propertyMode, string expected)
        {
            instance.PropertyMode = propertyMode;
            instance.PropertyUnderscoreToHyphen = false;

            var result = instance.ToCssClassName(PascalCase);

            result.Should().Be(expected);
        }

        [Fact]
        public void ToCssClassName_can_combine_the_settings()
        {
            instance.PropertyMode = CssBuilderNamingMode.KebabCase;
            instance.PropertyUnderscoreToHyphen = true;

            var result = instance.ToCssClassName(PascalCase_WithUnderScore);

            result.Should().Be("pascal-case--with-under-score");
        }

        private class Dummy
        {
            public bool PascalCase { get; set; }

            public bool PascalCase_WithUnderScore { get; set; }

        }
    }
}
