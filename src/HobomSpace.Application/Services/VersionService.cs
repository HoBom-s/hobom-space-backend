using HobomSpace.Application.Helpers;
using HobomSpace.Application.Models;
using HobomSpace.Application.Ports;
using HobomSpace.Application.Specifications;
using HobomSpace.Domain.Common;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Application.Services;

/// <summary>페이지 버전 이력 조회, 복원, diff 연산을 정의한다.</summary>
public interface IVersionService
{
    /// <summary>페이지의 버전 이력을 페이지네이션하여 조회한다.</summary>
    Task<PaginatedResult<PageVersion>> GetHistoryAsync(long pageId, int offset, int limit, CancellationToken ct = default);

    /// <summary>특정 버전 번호의 스냅샷을 조회한다.</summary>
    Task<Result<PageVersion>> GetVersionAsync(long pageId, int version, CancellationToken ct = default);

    /// <summary>특정 버전의 제목·본문으로 페이지를 되돌린다.</summary>
    Task<Result<Page>> RestoreVersionAsync(long pageId, int version, CancellationToken ct = default);

    /// <summary>두 버전 간의 라인 단위 diff를 계산한다.</summary>
    Task<Result<List<DiffEntry>>> DiffVersionsAsync(long pageId, int from, int to, CancellationToken ct = default);
}

/// <summary>페이지 버전 서비스 구현체.</summary>
public sealed class VersionService(IRepository<Page> pageRepo, IRepository<PageVersion> versionRepo, IUnitOfWork uow) : IVersionService
{
    public async Task<PaginatedResult<PageVersion>> GetHistoryAsync(long pageId, int offset, int limit, CancellationToken ct)
    {
        (offset, limit) = PaginatedResult<PageVersion>.Clamp(offset, limit);
        var items = await versionRepo.ListAsync(new PageVersionsByPageIdSpec(pageId, offset, limit), ct);
        var total = await versionRepo.CountAsync(new PageVersionCountByPageIdSpec(pageId), ct);
        return new PaginatedResult<PageVersion>(items, total, offset, limit);
    }

    public async Task<Result<PageVersion>> GetVersionAsync(long pageId, int version, CancellationToken ct)
    {
        var v = await versionRepo.FirstOrDefaultAsync(new PageVersionByNumberSpec(pageId, version), ct);
        if (v is null) return Result.Failure<PageVersion>(DomainErrors.PageVersion.NotFound(version));
        return v;
    }

    public async Task<Result<Page>> RestoreVersionAsync(long pageId, int version, CancellationToken ct)
    {
        var v = await versionRepo.FirstOrDefaultAsync(new PageVersionByNumberSpec(pageId, version), ct);
        if (v is null) return Result.Failure<Page>(DomainErrors.PageVersion.NotFound(version));

        var page = await pageRepo.FirstOrDefaultAsync(new PageByIdSpec(pageId), ct);
        if (page is null) return Result.Failure<Page>(DomainErrors.Page.NotFound(pageId));

        var result = page.Update(v.Title, v.Content, null);
        if (result.IsFailure) return Result.Failure<Page>(result.Error);

        await uow.SaveChangesAsync(ct);
        return page;
    }

    public async Task<Result<List<DiffEntry>>> DiffVersionsAsync(long pageId, int from, int to, CancellationToken ct)
    {
        var fromVer = await versionRepo.FirstOrDefaultAsync(new PageVersionByNumberSpec(pageId, from), ct);
        if (fromVer is null) return Result.Failure<List<DiffEntry>>(DomainErrors.PageVersion.NotFound(from));

        var toVer = await versionRepo.FirstOrDefaultAsync(new PageVersionByNumberSpec(pageId, to), ct);
        if (toVer is null) return Result.Failure<List<DiffEntry>>(DomainErrors.PageVersion.NotFound(to));

        return LineDiffHelper.ComputeDiff(fromVer.Content, toVer.Content);
    }
}
