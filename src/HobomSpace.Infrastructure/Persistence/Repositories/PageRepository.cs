using HobomSpace.Application.Helpers;
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

    public async Task<List<Page>> SearchAsync(string query, int offset, int limit, CancellationToken ct = default)
    {
        var escaped = LikeQueryHelper.EscapeLikePattern(query);
        return await db.Pages
            .Where(p => EF.Functions.ILike(p.Title, $"%{escaped}%", "\\") || EF.Functions.ILike(p.Content, $"%{escaped}%", "\\"))
            .OrderByDescending(p => p.UpdatedAt)
            .Skip(offset).Take(limit)
            .ToListAsync(ct);
    }

    public async Task<int> SearchCountAsync(string query, CancellationToken ct = default)
    {
        var escaped = LikeQueryHelper.EscapeLikePattern(query);
        return await db.Pages
            .CountAsync(p => EF.Functions.ILike(p.Title, $"%{escaped}%", "\\") || EF.Functions.ILike(p.Content, $"%{escaped}%", "\\"), ct);
    }

    public async Task<List<Page>> SearchBySpaceIdAsync(long spaceId, string query, int offset, int limit, CancellationToken ct = default)
    {
        var escaped = LikeQueryHelper.EscapeLikePattern(query);
        return await db.Pages
            .Where(p => p.SpaceId == spaceId)
            .Where(p => EF.Functions.ILike(p.Title, $"%{escaped}%", "\\") || EF.Functions.ILike(p.Content, $"%{escaped}%", "\\"))
            .OrderByDescending(p => p.UpdatedAt)
            .Skip(offset).Take(limit)
            .ToListAsync(ct);
    }

    public async Task<int> SearchBySpaceIdCountAsync(long spaceId, string query, CancellationToken ct = default)
    {
        var escaped = LikeQueryHelper.EscapeLikePattern(query);
        return await db.Pages
            .Where(p => p.SpaceId == spaceId)
            .CountAsync(p => EF.Functions.ILike(p.Title, $"%{escaped}%", "\\") || EF.Functions.ILike(p.Content, $"%{escaped}%", "\\"), ct);
    }

    public async Task AddAsync(Page page, CancellationToken ct = default)
        => await db.Pages.AddAsync(page, ct);

    public void Remove(Page page)
        => db.Pages.Remove(page);

    public async Task<Page?> GetDeletedByIdAsync(long id, CancellationToken ct = default)
        => await db.Pages.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == id && p.DeletedAt != null, ct);

    public async Task<List<Page>> GetDeletedBySpaceIdAsync(long spaceId, int offset, int limit, CancellationToken ct = default)
        => await db.Pages.IgnoreQueryFilters()
            .Where(p => p.SpaceId == spaceId && p.DeletedAt != null)
            .OrderByDescending(p => p.DeletedAt)
            .Skip(offset).Take(limit)
            .ToListAsync(ct);

    public async Task<int> CountDeletedBySpaceIdAsync(long spaceId, CancellationToken ct = default)
        => await db.Pages.IgnoreQueryFilters()
            .CountAsync(p => p.SpaceId == spaceId && p.DeletedAt != null, ct);

    public async Task<int> PurgeDeletedOlderThanAsync(DateTime cutoff, int batchSize, CancellationToken ct = default)
    {
        var pages = await db.Pages.IgnoreQueryFilters()
            .Where(p => p.DeletedAt != null && p.DeletedAt < cutoff)
            .Take(batchSize)
            .ToListAsync(ct);
        db.Pages.RemoveRange(pages);
        await db.SaveChangesAsync(ct);
        return pages.Count;
    }
}
