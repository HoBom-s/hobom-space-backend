using HobomSpace.Domain.Entities;

namespace HobomSpace.Application.Ports;

public interface IErrorEventRepository
{
    Task AddAsync(ErrorEvent errorEvent, CancellationToken ct = default);
    Task<ErrorEvent?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<List<ErrorEvent>> GetAllAsync(int offset, int limit, string? errorType = null, string? screen = null, CancellationToken ct = default);
    Task<int> CountAsync(string? errorType = null, string? screen = null, CancellationToken ct = default);
}
