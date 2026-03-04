namespace HobomSpace.Domain.Entities;

public sealed class Space
{
    public long Id { get; private set; }
    public string Key { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Space() { }

    public static Space Create(string key, string name, string? description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (key.Length > 32)
            throw new ArgumentException("Key must be 32 characters or less.", nameof(key));

        var now = DateTime.UtcNow;
        return new Space
        {
            Key = key.ToUpperInvariant(),
            Name = name.Trim(),
            Description = description?.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    public void Update(string name, string? description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
