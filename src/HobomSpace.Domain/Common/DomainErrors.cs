namespace HobomSpace.Domain.Common;

public static class DomainErrors
{
    public static class Space
    {
        public static Error NotFound(string key) => new("Space.NotFound", $"Space '{key}' not found.");
        public static Error AlreadyExists(string key) => new("Space.AlreadyExists", $"Space with key '{key}' already exists.");
        public static readonly Error KeyTooLong = new("Space.KeyTooLong", "Key must be 32 characters or less.");
        public static readonly Error KeyEmpty = new("Space.KeyEmpty", "Key cannot be null or whitespace.");
        public static readonly Error NameEmpty = new("Space.NameEmpty", "Name cannot be null or whitespace.");
    }

    public static class Page
    {
        public static Error NotFound(long id) => new("Page.NotFound", $"Page {id} not found.");
        public static Error NotInSpace(long id, string key) => new("Page.NotFound", $"Page {id} not found in space '{key}'.");
        public static Error ParentNotFound(long id) => new("Page.ParentNotFound", $"Parent page {id} not found.");
        public static Error ParentNotInTargetSpace(long id, string key) => new("Page.ParentNotFound", $"Parent page {id} not found in target space '{key}'.");
        public static readonly Error TitleEmpty = new("Page.TitleEmpty", "Title cannot be null or whitespace.");
        public static readonly Error ContentNull = new("Page.ContentNull", "Content cannot be null.");
    }

    public static class Comment
    {
        public static Error NotFound(long id) => new("Comment.NotFound", $"Comment with id {id} not found");
        public static Error ParentNotFound(long id) => new("Comment.ParentNotFound", $"Parent comment {id} not found.");
        public static readonly Error ParentOnDifferentPage = new("Comment.ParentOnDifferentPage", "Parent comment does not belong to the specified page.");
    }

    public static class Label
    {
        public static Error NotFound(long id) => new("Label.NotFound", $"Label {id} not found.");
        public static Error NotInSpace(long id) => new("Label.NotFound", $"Label {id} not found in this space.");
        public static Error AlreadyAssigned(long pageId) => new("Label.AlreadyAssigned", $"Label already assigned to page {pageId}.");
        public static Error NotAssigned(long labelId, long pageId) => new("Label.NotAssigned", $"Label {labelId} not found on page {pageId}.");
    }

    public static class PageVersion
    {
        public static Error NotFound(int version) => new("PageVersion.NotFound", $"PageVersion {version} not found");
    }

    public static class ErrorEvent
    {
        public static Error NotFound(long id) => new("ErrorEvent.NotFound", $"ErrorEvent with id {id} not found");
        public static Error InvalidType(string type) => new("ErrorEvent.InvalidType", $"Invalid error type: {type}. Must be one of: SERVER_RESPONSE, CLIENT_LOGIC");
    }
}
