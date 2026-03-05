using HobomSpace.Domain.Entities;

namespace HobomSpace.Application.Ports;

public interface IOutboxRepository
{
    Task<List<OutboxMessage>> FindByEventTypeAndStatusAsync(string eventType, string status, CancellationToken ct = default);
    Task<OutboxMessage?> FindByEventIdAsync(string eventId, CancellationToken ct = default);
    Task AddAsync(OutboxMessage message, CancellationToken ct = default);
}
