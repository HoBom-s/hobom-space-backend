using HobomSpace.Application.Ports;
using HobomSpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HobomSpace.Infrastructure.Persistence.Repositories;

public sealed class PageRepository(AppDbContext db) : IPageRepository
{
    public async Task<Page?> GetByIdAsync(long id, CancellationToken ct = default)
        => await db.Pages.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<List<Page>> GetBySpaceIdAsync(long spaceId, CancellationToken ct = default)
        => await db.Pages.Where(p => p.SpaceId == spaceId).OrderBy(p => p.Position).ToListAsync(ct);

    public async Task AddAsync(Page page, CancellationToken ct = default)
        => await db.Pages.AddAsync(page, ct);

    public void Remove(Page page)
        => db.Pages.Remove(page);
}
