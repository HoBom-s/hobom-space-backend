namespace HobomSpace.Domain.Entities;

public sealed class PageVersion
{
    public long Id { get; private set; }
    public long PageId { get; private set; }
    public int Version { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string? EditedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private PageVersion() { }

    public static PageVersion Create(long pageId, int version, string title, string content, string? editedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        return new PageVersion
        {
            PageId = pageId,
            Version = version + 1,
            Title = title,
            Content = content,
            EditedBy = editedBy,
        };
    }
}
