using HobomSpace.Domain.Entities;
using HobomSpace.Domain.ValueObjects;

namespace HobomSpace.Tests.Unit.Helpers;

internal static class EntityTestHelper
{
    public static Space CreateSpaceWithId(long id, string key = "TEST", string name = "Test Space")
    {
        var spaceKey = SpaceKey.Create(key).Value;
        var space = Space.Create(spaceKey, name, null).Value;
        typeof(Space).GetProperty(nameof(Space.Id))!.SetValue(space, id);
        return space;
    }

    public static Page CreatePageWithId(long id, long spaceId = 1, string title = "Test Page", string content = "content")
    {
        var space = CreateSpaceWithId(spaceId);
        var page = Page.Create(space, null, title, content).Value;
        typeof(Page).GetProperty(nameof(Page.Id))!.SetValue(page, id);
        return page;
    }

    public static Comment CreateCommentWithId(long id, long pageId = 1, long? parentCommentId = null, string content = "Test comment", string? author = "tester")
    {
        var page = CreatePageWithId(pageId);
        var comment = Comment.Create(page, parentCommentId, content, author, "TEST", null).Value;
        typeof(Comment).GetProperty(nameof(Comment.Id))!.SetValue(comment, id);
        return comment;
    }

    public static ErrorEvent CreateErrorEventWithId(long id, string message = "Test error", string screen = "/test", string errorType = "CLIENT_LOGIC")
    {
        var errorEvent = ErrorEvent.Create(message, null, screen, errorType, null, null);
        typeof(ErrorEvent).GetProperty(nameof(ErrorEvent.Id))!.SetValue(errorEvent, id);
        return errorEvent;
    }

    public static Label CreateLabelWithId(long id, long spaceId = 1, string name = "Test Label", string color = "#FF0000")
    {
        var hexColor = HexColor.Create(color).Value;
        var label = Label.Create(spaceId, name, hexColor).Value;
        typeof(Label).GetProperty(nameof(Label.Id))!.SetValue(label, id);
        return label;
    }

    public static PageLabel CreatePageLabelWithId(long id, long pageId = 1, long labelId = 1)
    {
        var pageLabel = PageLabel.Create(pageId, labelId);
        typeof(PageLabel).GetProperty(nameof(PageLabel.Id))!.SetValue(pageLabel, id);
        return pageLabel;
    }
}
