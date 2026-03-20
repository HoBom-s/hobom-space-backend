using FluentAssertions;
using HobomSpace.Domain.Common;
using HobomSpace.Domain.Entities;
using HobomSpace.Domain.ValueObjects;
using HobomSpace.Tests.Unit.Helpers;

namespace HobomSpace.Tests.Unit.Domain;

public class PageSoftDeleteTests
{
    private static Space ValidSpace(long id = 1, string key = "TEST")
        => EntityTestHelper.CreateSpaceWithId(id, key);

    private static Page ValidPage(Space? space = null)
        => Page.Create(space ?? ValidSpace(), null, "Title", "Content").Value;

    [Fact]
    public void SoftDelete_SetsDeletedAtAndDeletedBy()
    {
        var page = ValidPage();

        page.SoftDelete("TEST", "user1");

        page.DeletedAt.Should().NotBeNull();
        page.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        page.DeletedBy.Should().Be("user1");
    }

    [Fact]
    public void SoftDelete_WithNullActor_SetsDeletedByNull()
    {
        var page = ValidPage();

        page.SoftDelete("TEST", null);

        page.DeletedAt.Should().NotBeNull();
        page.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void Restore_ClearsDeletedAtAndDeletedBy()
    {
        var page = ValidPage();
        page.SoftDelete("TEST", "user1");

        page.Restore();

        page.DeletedAt.Should().BeNull();
        page.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void MoveTo_UpdatesSpaceIdAndParentPageId()
    {
        var sourceSpace = ValidSpace(1, "SRC");
        var targetSpace = ValidSpace(2, "TGT");
        var page = Page.Create(sourceSpace, null, "Title", "Content").Value;

        var result = page.MoveTo(targetSpace, null, "user1");

        result.IsSuccess.Should().BeTrue();
        page.SpaceId.Should().Be(targetSpace.Id);
        page.ParentPageId.Should().BeNull();
        page.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MoveTo_WithParentPage_SetsParentPageId()
    {
        var targetSpace = ValidSpace(2, "TGT");
        var parentPage = Page.Create(targetSpace, null, "Parent", "Content").Value;
        typeof(Page).GetProperty(nameof(Page.Id))!.SetValue(parentPage, 20L);

        var sourceSpace = ValidSpace(1, "SRC");
        var page = Page.Create(sourceSpace, null, "Title", "Content").Value;

        var result = page.MoveTo(targetSpace, parentPage, null);

        result.IsSuccess.Should().BeTrue();
        page.SpaceId.Should().Be(targetSpace.Id);
        page.ParentPageId.Should().Be(20);
    }

    [Fact]
    public void MoveTo_WithNullParent_SetsParentToNull()
    {
        var sourceSpace = ValidSpace(1, "SRC");
        var page = Page.Create(sourceSpace, 10, "Title", "Content").Value;
        var targetSpace = ValidSpace(2, "TGT");

        var result = page.MoveTo(targetSpace, null, null);

        result.IsSuccess.Should().BeTrue();
        page.ParentPageId.Should().BeNull();
    }

    [Fact]
    public void MoveTo_WithParentInDifferentSpace_ReturnsFailure()
    {
        var space1 = ValidSpace(1, "S1");
        var space2 = ValidSpace(2, "S2");
        var parentInSpace1 = Page.Create(space1, null, "Parent", "Content").Value;

        var page = Page.Create(space1, null, "Title", "Content").Value;

        var result = page.MoveTo(space2, parentInSpace1, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Page.ParentNotFound");
    }
}
