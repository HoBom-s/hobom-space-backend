using FluentAssertions;
using HobomSpace.Domain.Common;
using HobomSpace.Domain.Entities;
using HobomSpace.Domain.ValueObjects;

namespace HobomSpace.Tests.Unit.Domain;

public class LabelTests
{
    private static HexColor ValidColor(string hex = "#FF0000") => HexColor.Create(hex).Value;

    [Fact]
    public void Create_WithValidArgs_ReturnsLabel()
    {
        var result = Label.Create(1, "Bug", ValidColor());

        result.IsSuccess.Should().BeTrue();
        var label = result.Value;
        label.SpaceId.Should().Be(1);
        label.Name.Should().Be("Bug");
        label.Color.Should().Be("#FF0000");
        label.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_TrimsName()
    {
        var result = Label.Create(1, " Bug ", ValidColor());

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Bug");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ReturnsFailure(string? name)
    {
        var result = Label.Create(1, name!, ValidColor());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Label.NameEmpty");
    }

    [Fact]
    public void Create_WithNameTooLong_ReturnsFailure()
    {
        var result = Label.Create(1, new string('a', 51), ValidColor());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Label.NameTooLong");
    }

    [Fact]
    public void Update_WithValidArgs_UpdatesFields()
    {
        var label = Label.Create(1, "Bug", ValidColor()).Value;

        var result = label.Update("Feature", ValidColor("#00FF00"));

        result.IsSuccess.Should().BeTrue();
        label.Name.Should().Be("Feature");
        label.Color.Should().Be("#00FF00");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidName_ReturnsFailure(string? name)
    {
        var label = Label.Create(1, "Bug", ValidColor()).Value;

        var result = label.Update(name!, ValidColor());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Label.NameEmpty");
    }

    [Fact]
    public void Update_WithNameTooLong_ReturnsFailure()
    {
        var label = Label.Create(1, "Bug", ValidColor()).Value;

        var result = label.Update(new string('a', 51), ValidColor());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Label.NameTooLong");
    }

    [Fact]
    public void Update_TrimsName()
    {
        var label = Label.Create(1, "Bug", ValidColor()).Value;

        var result = label.Update(" Feature ", ValidColor("#00F"));

        result.IsSuccess.Should().BeTrue();
        label.Name.Should().Be("Feature");
        label.Color.Should().Be("#00F");
    }
}
