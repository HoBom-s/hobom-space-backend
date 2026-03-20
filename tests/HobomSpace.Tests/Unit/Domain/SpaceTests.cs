using FluentAssertions;
using HobomSpace.Domain.Common;
using HobomSpace.Domain.Entities;
using HobomSpace.Domain.ValueObjects;

namespace HobomSpace.Tests.Unit.Domain;

public class SpaceTests
{
    private static SpaceKey ValidKey(string key = "DEV") => SpaceKey.Create(key).Value;

    [Fact]
    public void Create_WithValidArgs_ReturnsSpace()
    {
        var result = Space.Create(ValidKey("dev"), "Development", "Dev space");

        result.IsSuccess.Should().BeTrue();
        var space = result.Value;
        space.Key.Should().Be("DEV");
        space.Name.Should().Be("Development");
        space.Description.Should().Be("Dev space");
        space.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        space.UpdatedAt.Should().Be(space.CreatedAt);
    }

    [Fact]
    public void Create_UppercasesKey()
    {
        var result = Space.Create(ValidKey("abc"), "Name", null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Key.Should().Be("ABC");
    }

    [Fact]
    public void Create_TrimsNameAndDescription()
    {
        var result = Space.Create(ValidKey("KEY"), "  padded  ", "  desc  ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("padded");
        result.Value.Description.Should().Be("desc");
    }

    [Fact]
    public void Create_WithNullDescription_SetsNull()
    {
        var result = Space.Create(ValidKey(), "Name", null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Description.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ReturnsFailure(string? name)
    {
        var result = Space.Create(ValidKey(), name!, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Space.NameEmpty");
    }

    [Fact]
    public void Update_WithValidArgs_UpdatesFields()
    {
        var space = Space.Create(ValidKey(), "Old", "Old desc").Value;
        var createdAt = space.CreatedAt;

        var result = space.Update("New", "New desc");

        result.IsSuccess.Should().BeTrue();
        space.Name.Should().Be("New");
        space.Description.Should().Be("New desc");
        space.UpdatedAt.Should().BeOnOrAfter(createdAt);
    }

    [Fact]
    public void Update_TrimsValues()
    {
        var space = Space.Create(ValidKey(), "Name", null).Value;

        var result = space.Update("  trimmed  ", "  desc  ");

        result.IsSuccess.Should().BeTrue();
        space.Name.Should().Be("trimmed");
        space.Description.Should().Be("desc");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidName_ReturnsFailure(string? name)
    {
        var space = Space.Create(ValidKey(), "Name", null).Value;

        var result = space.Update(name!, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Space.NameEmpty");
    }
}
