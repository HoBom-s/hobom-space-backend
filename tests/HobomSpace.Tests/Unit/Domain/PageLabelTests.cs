using FluentAssertions;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Tests.Unit.Domain;

public class PageLabelTests
{
    [Fact]
    public void Create_WithValidArgs_ReturnsPageLabel()
    {
        var pageLabel = PageLabel.Create(1, 2);

        pageLabel.PageId.Should().Be(1);
        pageLabel.LabelId.Should().Be(2);
        pageLabel.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidPageId_Throws(long pageId)
    {
        var act = () => PageLabel.Create(pageId, 1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidLabelId_Throws(long labelId)
    {
        var act = () => PageLabel.Create(1, labelId);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
