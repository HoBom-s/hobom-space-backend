using HobomSpace.Application.Ports;
using HobomSpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HobomSpace.Infrastructure.Persistence.Repositories;

public sealed class PageVersionRepository(AppDbContext db) : IPageVersionRepository
{
    public async Task<List<PageVersion>> GetByPageIdAsync(long pageId, CancellationToken ct = default)
        => await db.PageVersions.Where(v => v.PageId == pageId).ToListAsync(ct);

    public async Task<List<PageVersion>> GetByPageIdAsync(long pageId, int offset, int limit, CancellationToken ct = default)
        => await db.PageVersions.Where(v => v.PageId == pageId)
            .OrderByDescending(v => v.Version).Skip(offset).Take(limit).ToListAsync(ct);

    public async Task<int> CountByPageIdAsync(long pageId, CancellationToken ct = default)
        => await db.PageVersions.CountAsync(v => v.PageId == pageId, ct);

    public async Task<PageVersion?> GetByVersionAsync(long pageId, int version, CancellationToken ct = default)
        => await db.PageVersions.FirstOrDefaultAsync(v => v.PageId == pageId && v.Version == version, ct);

    public async Task AddAsync(PageVersion pageVersion, CancellationToken ct = default)
        => await db.PageVersions.AddAsync(pageVersion, ct);
}
