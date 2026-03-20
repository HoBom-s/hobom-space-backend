namespace HobomSpace.Domain.Entities;

/// <summary>
/// Transactional Outbox 패턴의 메시지. 도메인 이벤트를 DB 트랜잭션과 함께 저장하고,
/// 별도 프로세서(event-processor)가 비동기로 소비한다.
/// </summary>
public sealed class OutboxMessage
{
    public long Id { get; private set; }

    /// <summary>이벤트 고유 ID (UUID).</summary>
    public string EventId { get; private set; } = string.Empty;

    /// <summary>이벤트 유형 (예: SPACE_EVENT).</summary>
    public string EventType { get; private set; } = string.Empty;

    /// <summary>JSON 직렬화된 이벤트 페이로드.</summary>
    public string Payload { get; private set; } = string.Empty;

    /// <summary>처리 상태: PENDING → SENT 또는 FAILED.</summary>
    public string Status { get; private set; } = string.Empty;

    public int RetryCount { get; private set; }
    public DateTime? SentAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    public string? LastError { get; private set; }

    /// <summary>낙관적 동시성 제어를 위한 버전.</summary>
    public int Version { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private OutboxMessage() { }

    /// <summary>PENDING 상태의 새 Outbox 메시지를 생성한다.</summary>
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

    /// <summary>메시지를 SENT 상태로 전환한다.</summary>
    public void MarkAsSent()
    {
        Status = "SENT";
        SentAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>메시지를 FAILED 상태로 전환하고 에러 정보를 기록한다.</summary>
    public void MarkAsFailed(string error)
    {
        Status = "FAILED";
        FailedAt = DateTime.UtcNow;
        LastError = error;
        RetryCount++;
        UpdatedAt = DateTime.UtcNow;
    }
}
