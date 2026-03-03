using HobomSpace.Domain.Entities;

namespace HobomSpace.Application.Ports;

public interface IPageRepository
{
    Task<Page?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<List<Page>> GetBySpaceIdAsync(long spaceId, CancellationToken ct = default);
    Task AddAsync(Page page, CancellationToken ct = default);
    void Remove(Page page);
    Task SaveChangesAsync(CancellationToken ct = default);
}
