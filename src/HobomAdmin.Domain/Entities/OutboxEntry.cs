using HobomAdmin.Domain.Enums;

namespace HobomAdmin.Domain.Entities;

public class OutboxEntry
{
    public string Id { get; init; } = string.Empty;
    public MessageType MessageType { get; init; }
    public OutboxStatus Status { get; init; }
    public string Payload { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? SentAt { get; init; }
}
