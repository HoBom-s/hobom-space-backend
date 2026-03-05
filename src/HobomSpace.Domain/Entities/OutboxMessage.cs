namespace HobomSpace.Domain.Entities;

public sealed class OutboxMessage
{
    public long Id { get; private set; }
    public string EventId { get; private set; } = string.Empty;
    public string EventType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public int RetryCount { get; private set; }
    public DateTime? SentAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    public string? LastError { get; private set; }
    public int Version { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private OutboxMessage() { }

    public static OutboxMessage Create(string eventType, string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);

        return new OutboxMessage
        {
            EventId = Guid.NewGuid().ToString(),
            EventType = eventType,
            Payload = payload,
            Status = "PENDING",
            RetryCount = 0,
            Version = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public void MarkAsSent()
    {
        Status = "SENT";
        SentAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string error)
    {
        Status = "FAILED";
        FailedAt = DateTime.UtcNow;
        LastError = error;
        RetryCount++;
        UpdatedAt = DateTime.UtcNow;
    }
}
