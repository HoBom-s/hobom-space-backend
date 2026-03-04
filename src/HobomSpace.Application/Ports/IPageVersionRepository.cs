using HobomSpace.Domain.Entities;

namespace HobomSpace.Application.Ports;

public interface IPageVersionRepository
{
    Task<List<PageVersion>> GetByPageIdAsync(long pageId, CancellationToken ct = default);
    Task<List<PageVersion>> GetByPageIdAsync(long pageId, int offset, int limit, CancellationToken ct = default);
    Task<int> CountByPageIdAsync(long pageId, CancellationToken ct = default);
    Task<PageVersion?> GetByVersionAsync(long pageId, int version, CancellationToken ct = default);
    Task AddAsync(PageVersion pageVersion, CancellationToken ct = default);
}
