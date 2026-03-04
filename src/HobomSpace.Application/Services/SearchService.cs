using HobomSpace.Application.Models;
using HobomSpace.Application.Ports;
using HobomSpace.Domain.Entities;
using HobomSpace.Domain.Exceptions;

namespace HobomSpace.Application.Services;

public interface ISearchService
{
    Task<PaginatedResult<Page>> SearchPagesAsync(string query, int offset, int limit, CancellationToken ct = default);
    Task<PaginatedResult<Page>> SearchPagesInSpaceAsync(string spaceKey, string query, int offset, int limit, CancellationToken ct = default);
}

public sealed class SearchService(
    IPageRepository pageRepo,
    ISpaceRepository spaceRepo) : ISearchService
{
    public async Task<PaginatedResult<Page>> SearchPagesAsync(string query, int offset, int limit, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        (offset, limit) = PaginatedResult<Page>.Clamp(offset, limit);
        var trimmed = query.Trim();
        var items = await pageRepo.SearchAsync(trimmed, offset, limit, ct);
        var total = await pageRepo.SearchCountAsync(trimmed, ct);
        return new PaginatedResult<Page>(items, total, offset, limit);
    }

    public async Task<PaginatedResult<Page>> SearchPagesInSpaceAsync(string spaceKey, string query, int offset, int limit, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        (offset, limit) = PaginatedResult<Page>.Clamp(offset, limit);
        var space = await spaceRepo.GetByKeyAsync(spaceKey, ct)
            ?? throw new NotFoundException($"Space '{spaceKey}' not found");
        var trimmed = query.Trim();
        var items = await pageRepo.SearchBySpaceIdAsync(space.Id, trimmed, offset, limit, ct);
        var total = await pageRepo.SearchBySpaceIdCountAsync(space.Id, trimmed, ct);
        return new PaginatedResult<Page>(items, total, offset, limit);
    }
}
