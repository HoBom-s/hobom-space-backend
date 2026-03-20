using HobomSpace.Domain.Entities;

namespace HobomSpace.Application.Ports;

/// <summary>OutboxMessage 엔티티의 영속성 포트.</summary>
public interface IOutboxRepository
{
    /// <summary>이벤트 타입과 상태로 Outbox 메시지를 조회한다.</summary>
    Task<List<OutboxMessage>> FindByEventTypeAndStatusAsync(string eventType, string status, CancellationToken ct = default);

    /// <summary>이벤트 ID로 Outbox 메시지를 조회한다.</summary>
    Task<OutboxMessage?> FindByEventIdAsync(string eventId, CancellationToken ct = default);

    /// <summary>새 Outbox 메시지를 추가한다.</summary>
    Task AddAsync(OutboxMessage message, CancellationToken ct = default);

    /// <summary>cutoff 이전의 메시지를 배치 단위로 삭제한다. 삭제된 수를 반환한다.</summary>
    Task<int> DeleteOlderThanAsync(DateTime cutoff, int batchSize, CancellationToken ct = default);
}
