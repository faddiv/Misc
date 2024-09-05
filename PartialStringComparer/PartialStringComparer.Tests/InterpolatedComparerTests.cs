using FluentAssertions;

namespace PartialStringComparer.Tests;

public class InterpolatedComparerTests
{
    [Fact]
    public void NullsAreNotEqualToEmpty()
    {
        string? s1 = null;

        var result = InterpolatedComparer.Equals(s1, $"");

        result.Should().BeFalse();
    }

    [Fact]
    public void OnlyStringLiteral()
    {
        var s1 = "fist";

        var result = InterpolatedComparer.Equals(s1, $"fist");

        result.Should().BeTrue();
    }

    [Fact]
    public void WithOneStringPlaceholder()
    {
        var s1 = "fist of the furry";
        var of = "of";

        var result = InterpolatedComparer.Equals(s1, $"fist {of} the furry");

        result.Should().BeTrue();
    }

    [Fact]
    public void WithMultipleStringPlaceholder()
    {
        var s1 = "fist of the furry";
        var fist = "fist";
        var of = "of";
        var the = "the";
        var furry = "furry";

        var result = InterpolatedComparer.Equals(s1, $"{fist} {of} {the} {furry}");

        result.Should().BeTrue();
    }

    [Fact]
    public void WithMixedPlaceholder()
    {
        var s1 = "This is TheValue: 12543";
        var @enum = SomeEnum.TheValue;
        var number = 12543;

        var result = InterpolatedComparer.Equals(s1, $"This is {@enum}: {number}");

        result.Should().BeTrue();
    }

    [Fact]
    public void NullPlaceholdersIgnored()
    {
        var s1 = "This is : ";
        SomeEnum? @enum = null;
        int? number = null;
        object? obj = null;

        var result = InterpolatedComparer.Equals(s1, $"This{obj} is {@enum}: {number}");

        result.Should().BeTrue();
    }

    public enum SomeEnum
    {
        TheValue
    }
}
