using HobomSpace.Domain.Entities;

namespace HobomSpace.Application.Ports;

public interface ICommentRepository
{
    Task<Comment?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<List<Comment>> GetByPageIdAsync(long pageId, CancellationToken ct = default);
    Task<List<Comment>> GetByPageIdAsync(long pageId, int offset, int limit, CancellationToken ct = default);
    Task<int> CountByPageIdAsync(long pageId, CancellationToken ct = default);
    Task AddAsync(Comment comment, CancellationToken ct = default);
    void Remove(Comment comment);
}
