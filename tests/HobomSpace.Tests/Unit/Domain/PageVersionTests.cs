using FluentAssertions;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Tests.Unit.Domain;

public class PageVersionTests
{
    [Fact]
    public void Create_WithValidArgs_ReturnsPageVersion()
    {
        var pv = PageVersion.Create(1, 0, "Title", "Content", "user1");

        pv.PageId.Should().Be(1);
        pv.Version.Should().Be(1);
        pv.Title.Should().Be("Title");
        pv.Content.Should().Be("Content");
        pv.EditedBy.Should().Be("user1");
    }

    [Fact]
    public void Create_IncrementsVersionByOne()
    {
        var pv = PageVersion.Create(1, 3, "Title", "Content", null);

        pv.Version.Should().Be(4);
    }

    [Fact]
    public void Create_WithNullEditedBy_SetsNull()
    {
        var pv = PageVersion.Create(1, 0, "Title", "Content", null);

        pv.EditedBy.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidTitle_ThrowsArgumentException(string? title)
    {
        var act = () => PageVersion.Create(1, 0, title!, "Content", null);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidContent_ThrowsArgumentException(string? content)
    {
        var act = () => PageVersion.Create(1, 0, "Title", content!, null);

        act.Should().Throw<ArgumentException>();
    }
}
