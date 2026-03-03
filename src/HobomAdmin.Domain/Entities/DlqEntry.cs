namespace HobomAdmin.Domain.Entities;

public class DlqEntry
{
    public string Key { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string EventId { get; init; } = string.Empty;
}
