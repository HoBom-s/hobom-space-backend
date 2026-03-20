using FluentAssertions;
using HobomSpace.Domain.Common;
using HobomSpace.Domain.ValueObjects;

namespace HobomSpace.Tests.Unit.Domain;

public class SpaceKeyTests
{
    [Fact]
    public void Create_WithValidKey_ReturnsSuccess()
    {
        var result = SpaceKey.Create("dev");

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("DEV");
    }

    [Fact]
    public void Create_NormalizesToUpperCase()
    {
        var result = SpaceKey.Create("myProject");

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("MYPROJECT");
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var result = SpaceKey.Create("  key  ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("KEY");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrEmpty_ReturnsFailure(string? key)
    {
        var result = SpaceKey.Create(key);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Space.KeyEmpty");
    }

    [Fact]
    public void Create_WithKeyOver32Chars_ReturnsFailure()
    {
        var longKey = new string('A', 33);

        var result = SpaceKey.Create(longKey);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Space.KeyTooLong");
    }

    [Fact]
    public void Create_With32Chars_Succeeds()
    {
        var key = new string('A', 32);

        var result = SpaceKey.Create(key);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(key);
    }

    [Fact]
    public void ImplicitConversion_ReturnsStringValue()
    {
        var spaceKey = SpaceKey.Create("test").Value;

        string value = spaceKey;

        value.Should().Be("TEST");
    }

    [Fact]
    public void Equals_SameKey_ReturnsTrue()
    {
        var a = SpaceKey.Create("abc").Value;
        var b = SpaceKey.Create("ABC").Value;

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentKey_ReturnsFalse()
    {
        var a = SpaceKey.Create("abc").Value;
        var b = SpaceKey.Create("xyz").Value;

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var spaceKey = SpaceKey.Create("dev").Value;

        spaceKey.ToString().Should().Be("DEV");
    }
}
