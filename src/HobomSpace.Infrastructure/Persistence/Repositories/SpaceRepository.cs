using HobomSpace.Application.Ports;
using HobomSpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HobomSpace.Infrastructure.Persistence.Repositories;

public sealed class SpaceRepository(AppDbContext db) : ISpaceRepository
{
    public async Task<Space?> GetByIdAsync(long id, CancellationToken ct = default)
        => await db.Spaces.FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<Space?> GetByKeyAsync(string key, CancellationToken ct = default)
        => await db.Spaces.FirstOrDefaultAsync(s => s.Key == key.ToUpperInvariant(), ct);

    public async Task<List<Space>> GetAllAsync(CancellationToken ct = default)
        => await db.Spaces.OrderBy(s => s.Name).ToListAsync(ct);

    public async Task AddAsync(Space space, CancellationToken ct = default)
        => await db.Spaces.AddAsync(space, ct);

    public void Remove(Space space)
        => db.Spaces.Remove(space);
}
