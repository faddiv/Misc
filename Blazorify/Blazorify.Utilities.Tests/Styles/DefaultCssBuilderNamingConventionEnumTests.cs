using Blazorify.Utilities.Styling;
using FluentAssertions;
using Xunit;

namespace Blazorify.Utilities.Tests.Styles
{
    public class DefaultCssBuilderNamingConventionEnumTests
    {
        public DefaultCssBuilderNamingConvention instance = new DefaultCssBuilderNamingConvention();

        public Dummy PascalCase = Dummy.PascalCase;

        public Dummy PascalCase_WithUnderScore = Dummy.PascalCase_WithUnderScore;

        [Fact]
        public void DefaultValue_is_correct()
        {
            instance.EnumMode.Should().Be(CssBuilderNamingMode.KebabCase);
            instance.EnumUnderscoreToHyphen.Should().BeTrue();
        }

        [Theory]
        [InlineData(false, "PascalCase_WithUnderScore")]
        [InlineData(true, "PascalCase-WithUnderScore")]
        public void ToCssClassName_uses_EnumUnderscoreToHyphen_value(
            bool EnumUnderscoreToHyphen, string expected)
        {
            instance.EnumMode = CssBuilderNamingMode.None;
            instance.EnumUnderscoreToHyphen = EnumUnderscoreToHyphen;

            var result = instance.ToCssClassName(PascalCase_WithUnderScore);

            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(CssBuilderNamingMode.None, "PascalCase")]
        [InlineData(CssBuilderNamingMode.KebabCase, "pascal-case")]
        public void ToCssClassName_uses_EnumMode_value(
            CssBuilderNamingMode EnumMode, string expected)
        {
            instance.EnumMode = EnumMode;
            instance.EnumUnderscoreToHyphen = false;

            var result = instance.ToCssClassName(PascalCase);

            result.Should().Be(expected);
        }

        [Fact]
        public void ToCssClassName_can_combine_the_settings()
        {
            instance.EnumMode = CssBuilderNamingMode.KebabCase;
            instance.EnumUnderscoreToHyphen = true;

            var result = instance.ToCssClassName(PascalCase_WithUnderScore);

            result.Should().Be("pascal-case--with-under-score");
        }

        public enum Dummy
        {
            PascalCase,
            PascalCase_WithUnderScore
        }

    }
}
