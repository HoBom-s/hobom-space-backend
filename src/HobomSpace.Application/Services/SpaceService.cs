using HobomSpace.Application.Ports;
using HobomSpace.Domain.Entities;
using HobomSpace.Domain.Exceptions;

namespace HobomSpace.Application.Services;

public interface ISpaceService
{
    Task<Space> CreateAsync(string key, string name, string? description, CancellationToken ct = default);
    Task<List<Space>> GetAllAsync(CancellationToken ct = default);
    Task<Space> GetByKeyAsync(string key, CancellationToken ct = default);
    Task<Space> UpdateAsync(string key, string name, string? description, CancellationToken ct = default);
    Task DeleteAsync(string key, CancellationToken ct = default);
}

public sealed class SpaceService(ISpaceRepository repo, IUnitOfWork uow) : ISpaceService
{
    public async Task<Space> CreateAsync(string key, string name, string? description, CancellationToken ct = default)
    {
        var existing = await repo.GetByKeyAsync(key, ct);
        if (existing is not null)
            throw new ConflictException($"Space with key '{key.ToUpperInvariant()}' already exists.");

        var space = Space.Create(key, name, description);
        await repo.AddAsync(space, ct);
        await uow.SaveChangesAsync(ct);
        return space;
    }

    public async Task<List<Space>> GetAllAsync(CancellationToken ct = default)
        => await repo.GetAllAsync(ct);

    public async Task<Space> GetByKeyAsync(string key, CancellationToken ct = default)
        => await repo.GetByKeyAsync(key, ct)
           ?? throw new NotFoundException($"Space '{key}' not found.");

    public async Task<Space> UpdateAsync(string key, string name, string? description, CancellationToken ct = default)
    {
        var space = await repo.GetByKeyAsync(key, ct)
                    ?? throw new NotFoundException($"Space '{key}' not found.");

        space.Update(name, description);
        await uow.SaveChangesAsync(ct);
        return space;
    }

    public async Task DeleteAsync(string key, CancellationToken ct = default)
    {
        var space = await repo.GetByKeyAsync(key, ct)
                    ?? throw new NotFoundException($"Space '{key}' not found.");

        repo.Remove(space);
        await uow.SaveChangesAsync(ct);
    }
}
