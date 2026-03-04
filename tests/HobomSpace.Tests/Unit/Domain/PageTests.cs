using FluentAssertions;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Tests.Unit.Domain;

public class PageTests
{
    [Fact]
    public void Create_WithValidArgs_ReturnsPage()
    {
        var page = Page.Create(1, null, "Title", "Content", 0);

        page.SpaceId.Should().Be(1);
        page.ParentPageId.Should().BeNull();
        page.Title.Should().Be("Title");
        page.Content.Should().Be("Content");
        page.Position.Should().Be(0);
        page.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_TrimsTitle()
    {
        var page = Page.Create(1, null, "  padded  ", "Content");

        page.Title.Should().Be("padded");
    }

    [Fact]
    public void Create_WithParentPageId_SetsValue()
    {
        var page = Page.Create(1, 99, "Title", "Content");

        page.ParentPageId.Should().Be(99);
    }

    [Fact]
    public void Create_WithDefaultPosition_SetsZero()
    {
        var page = Page.Create(1, null, "Title", "Content");

        page.Position.Should().Be(0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidSpaceId_ThrowsArgumentOutOfRangeException(long spaceId)
    {
        var act = () => Page.Create(spaceId, null, "Title", "Content");

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidTitle_ThrowsArgumentException(string? title)
    {
        var act = () => Page.Create(1, null, title!, "Content");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNullContent_ThrowsArgumentNullException()
    {
        var act = () => Page.Create(1, null, "Title", null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_WithNegativePosition_ThrowsArgumentOutOfRangeException()
    {
        var act = () => Page.Create(1, null, "Title", "Content", -1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Update_WithValidArgs_UpdatesFields()
    {
        var page = Page.Create(1, null, "Old", "Old content");

        page.Update("New", "New content", 5);

        page.Title.Should().Be("New");
        page.Content.Should().Be("New content");
        page.Position.Should().Be(5);
    }

    [Fact]
    public void Update_WithNullPosition_DoesNotChangePosition()
    {
        var page = Page.Create(1, null, "Title", "Content", 3);

        page.Update("New", "New content", null);

        page.Position.Should().Be(3);
    }

    [Fact]
    public void Update_TrimsTitle()
    {
        var page = Page.Create(1, null, "Title", "Content");

        page.Update("  trimmed  ", "Content", null);

        page.Title.Should().Be("trimmed");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidTitle_ThrowsArgumentException(string? title)
    {
        var page = Page.Create(1, null, "Title", "Content");

        var act = () => page.Update(title!, "Content", null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Update_WithNullContent_ThrowsArgumentNullException()
    {
        var page = Page.Create(1, null, "Title", "Content");

        var act = () => page.Update("Title", null!, null);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Update_WithNegativePosition_ThrowsArgumentOutOfRangeException()
    {
        var page = Page.Create(1, null, "Title", "Content");

        var act = () => page.Update("Title", "Content", -1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
