namespace HobomSpace.Domain.Entities;

public sealed class Page
{
    public long Id { get; private set; }
    public long SpaceId { get; private set; }
    public long? ParentPageId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public int Position { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Page() { }

    public static Page Create(long spaceId, long? parentPageId, string title, string content, int position = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(spaceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentOutOfRangeException.ThrowIfNegative(position);

        var now = DateTime.UtcNow;
        return new Page
        {
            SpaceId = spaceId,
            ParentPageId = parentPageId,
            Title = title.Trim(),
            Content = content,
            Position = position,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    public void Update(string title, string content, int? position)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(content);

        if (position.HasValue)
            ArgumentOutOfRangeException.ThrowIfNegative(position.Value);

        Title = title.Trim();
        Content = content;
        if (position.HasValue)
            Position = position.Value;
        UpdatedAt = DateTime.UtcNow;
    }
}
