using FluentAssertions;
using HobomSpace.Domain.Common;
using HobomSpace.Domain.ValueObjects;

namespace HobomSpace.Tests.Unit.Domain;

public class HexColorTests
{
    [Theory]
    [InlineData("#FFF")]
    [InlineData("#FF0000")]
    [InlineData("#abc")]
    [InlineData("#ABCDEF")]
    [InlineData("#a1b2c3")]
    public void Create_WithValidHex_ReturnsSuccess(string hex)
    {
        var result = HexColor.Create(hex);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(hex);
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var result = HexColor.Create("  #FFF  ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("#FFF");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrEmpty_ReturnsFailure(string? color)
    {
        var result = HexColor.Create(color);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("HexColor.Empty");
    }

    [Theory]
    [InlineData("FF0000")]
    [InlineData("#GGG")]
    [InlineData("#FF")]
    [InlineData("#FF000001")]
    [InlineData("red")]
    [InlineData("#")]
    public void Create_WithInvalidFormat_ReturnsFailure(string color)
    {
        var result = HexColor.Create(color);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("HexColor.Invalid");
    }

    [Fact]
    public void ImplicitConversion_ReturnsStringValue()
    {
        var hexColor = HexColor.Create("#FF0000").Value;

        string value = hexColor;

        value.Should().Be("#FF0000");
    }

    [Fact]
    public void Equals_SameColor_CaseInsensitive_ReturnsTrue()
    {
        var a = HexColor.Create("#abc").Value;
        var b = HexColor.Create("#ABC").Value;

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentColor_ReturnsFalse()
    {
        var a = HexColor.Create("#FFF").Value;
        var b = HexColor.Create("#000").Value;

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var hexColor = HexColor.Create("#FF0000").Value;

        hexColor.ToString().Should().Be("#FF0000");
    }
}
