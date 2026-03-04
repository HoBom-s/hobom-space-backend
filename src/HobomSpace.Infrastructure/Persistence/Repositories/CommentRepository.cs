using HobomSpace.Application.Ports;
using HobomSpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HobomSpace.Infrastructure.Persistence.Repositories;

public sealed class CommentRepository(AppDbContext db) : ICommentRepository
{
    public async Task<Comment?> GetByIdAsync(long id, CancellationToken ct = default)
        => await db.Comments.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<List<Comment>> GetByPageIdAsync(long pageId, CancellationToken ct = default)
        => await db.Comments.Where(c => c.PageId == pageId).ToListAsync(ct);

    public async Task<List<Comment>> GetByPageIdAsync(long pageId, int offset, int limit, CancellationToken ct = default)
        => await db.Comments.Where(c => c.PageId == pageId)
            .OrderBy(c => c.CreatedAt).Skip(offset).Take(limit).ToListAsync(ct);

    public async Task<int> CountByPageIdAsync(long pageId, CancellationToken ct = default)
        => await db.Comments.CountAsync(c => c.PageId == pageId, ct);

    public async Task AddAsync(Comment comment, CancellationToken ct = default)
        => await db.Comments.AddAsync(comment, ct);

    public void Remove(Comment comment)
        => db.Comments.Remove(comment);
}
