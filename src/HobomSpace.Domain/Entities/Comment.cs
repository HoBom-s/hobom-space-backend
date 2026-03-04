namespace HobomSpace.Domain.Entities;

public sealed class Comment
{
    public long Id { get; private set; }
    public long PageId { get; private set; }
    public long? ParentCommentId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public string? Author { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Comment() { }

    public static Comment Create(long pageId, long? parentCommentId, string content, string? author)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        var now = DateTime.UtcNow;
        return new Comment
        {
            PageId = pageId,
            ParentCommentId = parentCommentId,
            Content = content,
            Author = author,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    public void Update(string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        Content = content;
        UpdatedAt = DateTime.UtcNow;
    }
}
