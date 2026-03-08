using HobomSpace.Application.Ports;
using HobomSpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HobomSpace.Infrastructure.Persistence.Repositories;

public sealed class ErrorEventRepository(AppDbContext db) : IErrorEventRepository
{
    public async Task AddAsync(ErrorEvent errorEvent, CancellationToken ct = default)
        => await db.ErrorEvents.AddAsync(errorEvent, ct);

    public async Task<ErrorEvent?> GetByIdAsync(long id, CancellationToken ct = default)
        => await db.ErrorEvents.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<List<ErrorEvent>> GetAllAsync(int offset, int limit, string? errorType = null, string? screen = null, CancellationToken ct = default)
    {
        var query = db.ErrorEvents.AsQueryable();

        if (!string.IsNullOrWhiteSpace(errorType))
            query = query.Where(e => e.ErrorType == errorType);

        if (!string.IsNullOrWhiteSpace(screen))
            query = query.Where(e => e.Screen == screen);

        return await query.OrderByDescending(e => e.CreatedAt)
            .Skip(offset).Take(limit).ToListAsync(ct);
    }

    public async Task<int> CountAsync(string? errorType = null, string? screen = null, CancellationToken ct = default)
    {
        var query = db.ErrorEvents.AsQueryable();

        if (!string.IsNullOrWhiteSpace(errorType))
            query = query.Where(e => e.ErrorType == errorType);

        if (!string.IsNullOrWhiteSpace(screen))
            query = query.Where(e => e.Screen == screen);

        return await query.CountAsync(ct);
    }
}
