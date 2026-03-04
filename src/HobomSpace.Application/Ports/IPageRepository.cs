using HobomSpace.Domain.Entities;

namespace HobomSpace.Application.Ports;

public interface IPageRepository
{
    Task<Page?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<List<Page>> GetBySpaceIdAsync(long spaceId, CancellationToken ct = default);
    Task<List<Page>> SearchAsync(string query, int offset, int limit, CancellationToken ct = default);
    Task<int> SearchCountAsync(string query, CancellationToken ct = default);
    Task<List<Page>> SearchBySpaceIdAsync(long spaceId, string query, int offset, int limit, CancellationToken ct = default);
    Task<int> SearchBySpaceIdCountAsync(long spaceId, string query, CancellationToken ct = default);
    Task AddAsync(Page page, CancellationToken ct = default);
    void Remove(Page page);
}
