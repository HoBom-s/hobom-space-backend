using HobomSpace.Application.Models;
using HobomSpace.Application.Ports;
using HobomSpace.Application.Specifications;
using HobomSpace.Domain.Common;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Application.Services;

/// <summary>휴지통(soft-deleted 페이지) 조회, 복원, 영구삭제 연산을 정의한다.</summary>
public interface ITrashService
{
    /// <summary>Space의 삭제된 페이지 목록을 페이지네이션하여 조회한다.</summary>
    Task<Result<PaginatedResult<Page>>> GetDeletedPagesAsync(string spaceKey, int offset, int limit, CancellationToken ct = default);

    /// <summary>삭제된 페이지를 복원한다.</summary>
    Task<Result<Page>> RestoreAsync(string spaceKey, long pageId, CancellationToken ct = default);

    /// <summary>삭제된 페이지를 영구 삭제한다.</summary>
    Task<Result> PermanentDeleteAsync(string spaceKey, long pageId, CancellationToken ct = default);
}

/// <summary>휴지통 서비스 구현체.</summary>
public sealed class TrashService(IRepository<Space> spaceRepo, IRepository<Page> pageRepo, IUnitOfWork uow) : ITrashService
{
    public async Task<Result<PaginatedResult<Page>>> GetDeletedPagesAsync(string spaceKey, int offset, int limit, CancellationToken ct)
    {
        var space = await spaceRepo.FirstOrDefaultAsync(new SpaceByKeySpec(spaceKey), ct);
        if (space is null) return Result.Failure<PaginatedResult<Page>>(DomainErrors.Space.NotFound(spaceKey));

        (offset, limit) = PaginatedResult<Page>.Clamp(offset, limit);
        var items = await pageRepo.ListAsync(new DeletedPagesBySpaceIdSpec(space.Id, offset, limit), ct);
        var total = await pageRepo.CountAsync(new DeletedPagesCountBySpaceIdSpec(space.Id), ct);
        return new PaginatedResult<Page>(items, total, offset, limit);
    }

    public async Task<Result<Page>> RestoreAsync(string spaceKey, long pageId, CancellationToken ct)
    {
        var space = await spaceRepo.FirstOrDefaultAsync(new SpaceByKeySpec(spaceKey), ct);
        if (space is null) return Result.Failure<Page>(DomainErrors.Space.NotFound(spaceKey));

        var page = await pageRepo.FirstOrDefaultAsync(new DeletedPageByIdSpec(pageId), ct);
        if (page is null) return Result.Failure<Page>(DomainErrors.Page.NotFound(pageId));
        if (page.SpaceId != space.Id) return Result.Failure<Page>(DomainErrors.Page.NotInSpace(pageId, spaceKey));

        page.Restore();
        await uow.SaveChangesAsync(ct);
        return page;
    }

    public async Task<Result> PermanentDeleteAsync(string spaceKey, long pageId, CancellationToken ct)
    {
        var space = await spaceRepo.FirstOrDefaultAsync(new SpaceByKeySpec(spaceKey), ct);
        if (space is null) return Result.Failure(DomainErrors.Space.NotFound(spaceKey));

        var page = await pageRepo.FirstOrDefaultAsync(new DeletedPageByIdSpec(pageId), ct);
        if (page is null) return Result.Failure(DomainErrors.Page.NotFound(pageId));
        if (page.SpaceId != space.Id) return Result.Failure(DomainErrors.Page.NotInSpace(pageId, spaceKey));

        await pageRepo.DeleteAsync(page, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
