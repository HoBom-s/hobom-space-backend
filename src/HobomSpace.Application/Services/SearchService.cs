using HobomSpace.Application.Models;
using HobomSpace.Application.Ports;
using HobomSpace.Application.Specifications;
using HobomSpace.Domain.Common;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Application.Services;

/// <summary>페이지 전문 검색 연산을 정의한다.</summary>
public interface ISearchService
{
    /// <summary>전체 Space에서 제목·본문 ILIKE 검색을 수행한다.</summary>
    Task<PaginatedResult<Page>> SearchPagesAsync(string query, int offset, int limit, CancellationToken ct = default);

    /// <summary>특정 Space 내에서 제목·본문 ILIKE 검색을 수행한다.</summary>
    Task<Result<PaginatedResult<Page>>> SearchPagesInSpaceAsync(string spaceKey, string query, int offset, int limit, CancellationToken ct = default);
}

/// <summary>페이지 검색 서비스 구현체. 읽기 전용 레포지토리를 사용한다.</summary>
public sealed class SearchService(IReadRepository<Space> spaceRepo, IReadRepository<Page> pageRepo) : ISearchService
{
    public async Task<PaginatedResult<Page>> SearchPagesAsync(string query, int offset, int limit, CancellationToken ct)
    {
        (offset, limit) = PaginatedResult<Page>.Clamp(offset, limit);
        var items = await pageRepo.ListAsync(new SearchPagesSpec(query, offset, limit), ct);
        var total = await pageRepo.CountAsync(new SearchPagesCountSpec(query), ct);
        return new PaginatedResult<Page>(items, total, offset, limit);
    }

    public async Task<Result<PaginatedResult<Page>>> SearchPagesInSpaceAsync(string spaceKey, string query, int offset, int limit, CancellationToken ct)
    {
        var space = await spaceRepo.FirstOrDefaultAsync(new SpaceByKeySpec(spaceKey), ct);
        if (space is null) return Result.Failure<PaginatedResult<Page>>(DomainErrors.Space.NotFound(spaceKey));

        (offset, limit) = PaginatedResult<Page>.Clamp(offset, limit);
        var items = await pageRepo.ListAsync(new SearchPagesBySpaceIdSpec(space.Id, query, offset, limit), ct);
        var total = await pageRepo.CountAsync(new SearchPagesBySpaceIdCountSpec(space.Id, query), ct);
        return new PaginatedResult<Page>(items, total, offset, limit);
    }
}
