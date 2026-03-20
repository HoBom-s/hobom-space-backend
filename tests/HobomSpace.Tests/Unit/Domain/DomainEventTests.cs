using FluentAssertions;
using HobomSpace.Domain.Entities;
using HobomSpace.Domain.Events;
using HobomSpace.Domain.ValueObjects;
using HobomSpace.Tests.Unit.Helpers;

namespace HobomSpace.Tests.Unit.Domain;

public class DomainEventTests
{
    private static Space ValidSpace(long id = 1, string key = "TEST")
        => EntityTestHelper.CreateSpaceWithId(id, key);

    [Fact]
    public void PageCreate_RaisesPageCreatedEvent()
    {
        var space = ValidSpace(1, "DEV");

        var page = Page.Create(space, null, "My Page", "Content", 0, "actor1").Value;

        page.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PageCreatedEvent>()
            .Which.Should().Match<PageCreatedEvent>(e =>
                e.SpaceId == space.Id &&
                e.SpaceKey == "DEV" &&
                e.Title == "My Page" &&
                e.ActorId == "actor1");
    }

    [Fact]
    public void PageSoftDelete_RaisesPageDeletedEvent()
    {
        var space = ValidSpace(1, "DEV");
        var page = Page.Create(space, null, "Title", "Content").Value;
        page.ClearDomainEvents();

        page.SoftDelete("DEV", "actor1");

        page.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PageDeletedEvent>()
            .Which.Should().Match<PageDeletedEvent>(e =>
                e.SpaceId == space.Id &&
                e.SpaceKey == "DEV" &&
                e.Title == "Title" &&
                e.ActorId == "actor1");
    }

    [Fact]
    public void PageMoveTo_RaisesPageMovedEvent()
    {
        var sourceSpace = ValidSpace(1, "SRC");
        var targetSpace = ValidSpace(2, "TGT");
        var page = Page.Create(sourceSpace, null, "Title", "Content").Value;
        page.ClearDomainEvents();

        var result = page.MoveTo(targetSpace, null, "actor1");

        result.IsSuccess.Should().BeTrue();
        page.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PageMovedEvent>()
            .Which.Should().Match<PageMovedEvent>(e =>
                e.SpaceId == targetSpace.Id &&
                e.SpaceKey == "TGT" &&
                e.Title == "Title" &&
                e.ActorId == "actor1");
    }

    [Fact]
    public void CommentCreate_RaisesCommentCreatedEvent()
    {
        var space = ValidSpace(1, "DEV");
        var page = Page.Create(space, null, "Title", "Content").Value;
        typeof(Page).GetProperty(nameof(Page.Id))!.SetValue(page, 42L);

        var comment = Comment.Create(page, null, "Great article!", "author1", "DEV", "actor1").Value;

        comment.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CommentCreatedEvent>()
            .Which.Should().Match<CommentCreatedEvent>(e =>
                e.PageId == 42 &&
                e.SpaceKey == "DEV" &&
                e.ContentPreview == "Great article!" &&
                e.ActorId == "actor1");
    }

    [Fact]
    public void CommentCreate_WithLongContent_TruncatesPreviewTo100Chars()
    {
        var page = Page.Create(ValidSpace(), null, "Title", "Content").Value;
        var longContent = new string('x', 200);

        var comment = Comment.Create(page, null, longContent, null, "TEST", null).Value;

        var evt = comment.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CommentCreatedEvent>().Subject;
        evt.ContentPreview.Should().HaveLength(100);
    }

    [Fact]
    public void SpaceCreate_RaisesSpaceCreatedEvent()
    {
        var key = SpaceKey.Create("DEV").Value;

        var space = Space.Create(key, "Development", "desc").Value;

        space.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SpaceCreatedEvent>()
            .Which.Should().Match<SpaceCreatedEvent>(e =>
                e.Key == "DEV" &&
                e.Name == "Development");
    }

    [Fact]
    public void DomainEvent_SetsOccurredAtToNow()
    {
        var space = Space.Create(SpaceKey.Create("TEST").Value, "Name", null).Value;

        var evt = space.DomainEvents.Should().ContainSingle().Subject;
        evt.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllEvents()
    {
        var space = Space.Create(SpaceKey.Create("TEST").Value, "Name", null).Value;
        space.DomainEvents.Should().NotBeEmpty();

        space.ClearDomainEvents();

        space.DomainEvents.Should().BeEmpty();
    }
}
