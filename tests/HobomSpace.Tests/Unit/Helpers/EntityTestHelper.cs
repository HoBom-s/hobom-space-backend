using HobomSpace.Domain.Entities;

namespace HobomSpace.Tests.Unit.Helpers;

internal static class EntityTestHelper
{
    public static Space CreateSpaceWithId(long id, string key = "TEST", string name = "Test Space")
    {
        var space = Space.Create(key, name, null);
        typeof(Space).GetProperty(nameof(Space.Id))!.SetValue(space, id);
        return space;
    }

    public static Page CreatePageWithId(long id, long spaceId = 1, string title = "Test Page", string content = "content")
    {
        var page = Page.Create(spaceId, null, title, content);
        typeof(Page).GetProperty(nameof(Page.Id))!.SetValue(page, id);
        return page;
    }

    public static Comment CreateCommentWithId(long id, long pageId = 1, long? parentCommentId = null, string content = "Test comment", string? author = "tester")
    {
        var comment = Comment.Create(pageId, parentCommentId, content, author);
        typeof(Comment).GetProperty(nameof(Comment.Id))!.SetValue(comment, id);
        return comment;
    }
}
