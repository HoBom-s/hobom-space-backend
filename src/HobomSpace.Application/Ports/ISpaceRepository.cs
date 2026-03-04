using HobomSpace.Domain.Entities;

namespace HobomSpace.Application.Ports;

public interface ISpaceRepository
{
    Task<Space?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Space?> GetByKeyAsync(string key, CancellationToken ct = default);
    Task<List<Space>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Space space, CancellationToken ct = default);
    void Remove(Space space);
}
