using FluentAssertions;
using HobomSpace.Domain.Common;
using HobomSpace.Domain.Entities;
using HobomSpace.Domain.ValueObjects;

namespace HobomSpace.Tests.Unit.Domain;

public class PageTests
{
    private static Space ValidSpace()
    {
        var key = SpaceKey.Create("TEST").Value;
        return Space.Create(key, "Test Space", null).Value;
    }

    [Fact]
    public void Create_WithValidArgs_ReturnsPage()
    {
        var space = ValidSpace();

        var result = Page.Create(space, null, "Title", "Content", 0);

        result.IsSuccess.Should().BeTrue();
        var page = result.Value;
        page.SpaceId.Should().Be(space.Id);
        page.ParentPageId.Should().BeNull();
        page.Title.Should().Be("Title");
        page.Content.Should().Be("Content");
        page.Position.Should().Be(0);
        page.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_TrimsTitle()
    {
        var result = Page.Create(ValidSpace(), null, "  padded  ", "Content");

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("padded");
    }

    [Fact]
    public void Create_WithParentPageId_SetsValue()
    {
        var result = Page.Create(ValidSpace(), 99, "Title", "Content");

        result.IsSuccess.Should().BeTrue();
        result.Value.ParentPageId.Should().Be(99);
    }

    [Fact]
    public void Create_WithDefaultPosition_SetsZero()
    {
        var result = Page.Create(ValidSpace(), null, "Title", "Content");

        result.IsSuccess.Should().BeTrue();
        result.Value.Position.Should().Be(0);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidTitle_ReturnsFailure(string? title)
    {
        var result = Page.Create(ValidSpace(), null, title!, "Content");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Page.TitleEmpty");
    }

    [Fact]
    public void Create_WithNullContent_ReturnsFailure()
    {
        var result = Page.Create(ValidSpace(), null, "Title", null!);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Page.ContentNull");
    }

    [Fact]
    public void Create_WithNegativePosition_ClampsToZero()
    {
        var result = Page.Create(ValidSpace(), null, "Title", "Content", -1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Position.Should().Be(0);
    }

    [Fact]
    public void Update_WithValidArgs_UpdatesFields()
    {
        var page = Page.Create(ValidSpace(), null, "Old", "Old content").Value;

        var result = page.Update("New", "New content", 5);

        result.IsSuccess.Should().BeTrue();
        page.Title.Should().Be("New");
        page.Content.Should().Be("New content");
        page.Position.Should().Be(5);
    }

    [Fact]
    public void Update_WithNullPosition_DoesNotChangePosition()
    {
        var page = Page.Create(ValidSpace(), null, "Title", "Content", 3).Value;

        var result = page.Update("New", "New content", null);

        result.IsSuccess.Should().BeTrue();
        page.Position.Should().Be(3);
    }

    [Fact]
    public void Update_TrimsTitle()
    {
        var page = Page.Create(ValidSpace(), null, "Title", "Content").Value;

        var result = page.Update("  trimmed  ", "Content", null);

        result.IsSuccess.Should().BeTrue();
        page.Title.Should().Be("trimmed");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidTitle_ReturnsFailure(string? title)
    {
        var page = Page.Create(ValidSpace(), null, "Title", "Content").Value;

        var result = page.Update(title!, "Content", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Page.TitleEmpty");
    }

    [Fact]
    public void Update_WithNullContent_ReturnsFailure()
    {
        var page = Page.Create(ValidSpace(), null, "Title", "Content").Value;

        var result = page.Update("Title", null!, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Page.ContentNull");
    }

    [Fact]
    public void Update_WithNegativePosition_ClampsToZero()
    {
        var page = Page.Create(ValidSpace(), null, "Title", "Content").Value;

        var result = page.Update("Title", "Content", -1);

        result.IsSuccess.Should().BeTrue();
        page.Position.Should().Be(0);
    }
}
