using FluentAssertions;
using HobomSpace.Domain.Common;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Tests.Unit.Domain;

public class PageVersionTests
{
    [Fact]
    public void Create_WithValidArgs_ReturnsPageVersion()
    {
        var result = PageVersion.Create(1, 0, "Title", "Content", "user1");

        result.IsSuccess.Should().BeTrue();
        var pv = result.Value;
        pv.PageId.Should().Be(1);
        pv.Version.Should().Be(1);
        pv.Title.Should().Be("Title");
        pv.Content.Should().Be("Content");
        pv.EditedBy.Should().Be("user1");
    }

    [Fact]
    public void Create_IncrementsVersionByOne()
    {
        var result = PageVersion.Create(1, 3, "Title", "Content", null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Version.Should().Be(4);
    }

    [Fact]
    public void Create_WithNullEditedBy_SetsNull()
    {
        var result = PageVersion.Create(1, 0, "Title", "Content", null);

        result.IsSuccess.Should().BeTrue();
        result.Value.EditedBy.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidTitle_ReturnsFailure(string? title)
    {
        var result = PageVersion.Create(1, 0, title!, "Content", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PageVersion.TitleEmpty");
    }

    [Fact]
    public void Create_WithEmptyContent_Succeeds()
    {
        var result = PageVersion.Create(1, 0, "Title", "", null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().BeEmpty();
    }

    [Fact]
    public void Create_SetsCreatedAtToNow()
    {
        var result = PageVersion.Create(1, 0, "Title", "Content", null);

        result.IsSuccess.Should().BeTrue();
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_SetsPageIdCorrectly()
    {
        var result = PageVersion.Create(42, 0, "Title", "Content", null);

        result.IsSuccess.Should().BeTrue();
        result.Value.PageId.Should().Be(42);
    }
}
