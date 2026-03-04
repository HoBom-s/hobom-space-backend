using FluentAssertions;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Tests.Unit.Domain;

public class SpaceTests
{
    [Fact]
    public void Create_WithValidArgs_ReturnsSpace()
    {
        var space = Space.Create("dev", "Development", "Dev space");

        space.Key.Should().Be("DEV");
        space.Name.Should().Be("Development");
        space.Description.Should().Be("Dev space");
        space.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        space.UpdatedAt.Should().Be(space.CreatedAt);
    }

    [Fact]
    public void Create_UppercasesKey()
    {
        var space = Space.Create("abc", "Name", null);

        space.Key.Should().Be("ABC");
    }

    [Fact]
    public void Create_TrimsNameAndDescription()
    {
        var space = Space.Create("KEY", "  padded  ", "  desc  ");

        space.Name.Should().Be("padded");
        space.Description.Should().Be("desc");
    }

    [Fact]
    public void Create_WithNullDescription_SetsNull()
    {
        var space = Space.Create("KEY", "Name", null);

        space.Description.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidKey_ThrowsArgumentException(string? key)
    {
        var act = () => Space.Create(key!, "Name", null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithKeyOver32Chars_ThrowsArgumentException()
    {
        var longKey = new string('A', 33);

        var act = () => Space.Create(longKey, "Name", null);

        act.Should().Throw<ArgumentException>().WithMessage("*32*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ThrowsArgumentException(string? name)
    {
        var act = () => Space.Create("KEY", name!, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Update_WithValidArgs_UpdatesFields()
    {
        var space = Space.Create("KEY", "Old", "Old desc");
        var createdAt = space.CreatedAt;

        space.Update("New", "New desc");

        space.Name.Should().Be("New");
        space.Description.Should().Be("New desc");
        space.UpdatedAt.Should().BeOnOrAfter(createdAt);
    }

    [Fact]
    public void Update_TrimsValues()
    {
        var space = Space.Create("KEY", "Name", null);

        space.Update("  trimmed  ", "  desc  ");

        space.Name.Should().Be("trimmed");
        space.Description.Should().Be("desc");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidName_ThrowsArgumentException(string? name)
    {
        var space = Space.Create("KEY", "Name", null);

        var act = () => space.Update(name!, null);

        act.Should().Throw<ArgumentException>();
    }
}
