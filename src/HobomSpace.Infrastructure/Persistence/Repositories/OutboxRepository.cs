using HobomSpace.Application.Ports;
using HobomSpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HobomSpace.Infrastructure.Persistence.Repositories;

public sealed class OutboxRepository(AppDbContext db) : IOutboxRepository
{
    public async Task<List<OutboxMessage>> FindByEventTypeAndStatusAsync(string eventType, string status, CancellationToken ct = default)
        => await db.OutboxMessages
            .Where(o => o.EventType == eventType && o.Status == status)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync(ct);

    public async Task<OutboxMessage?> FindByEventIdAsync(string eventId, CancellationToken ct = default)
        => await db.OutboxMessages.FirstOrDefaultAsync(o => o.EventId == eventId, ct);

    public async Task AddAsync(OutboxMessage message, CancellationToken ct = default)
        => await db.OutboxMessages.AddAsync(message, ct);
}
