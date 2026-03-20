using HobomSpace.Application.Ports;
using HobomSpace.Application.Specifications;
using HobomSpace.Domain.Common;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Application.Services;

/// <summary>페이지 CRUD, 이동, 복사 연산을 정의한다.</summary>
public interface IPageService
{
    /// <summary>Space에 새 페이지를 생성한다.</summary>
    Task<Result<Page>> CreateAsync(string spaceKey, string title, string content, long? parentPageId, int position, string? actorId, CancellationToken ct = default);

    /// <summary>페이지를 수정한다. 수정 전 현재 상태를 버전 스냅샷으로 저장한다.</summary>
    Task<Result<Page>> UpdateAsync(string spaceKey, long pageId, string title, string content, int? position, string? actorId, CancellationToken ct = default);

    /// <summary>페이지를 soft delete 처리한다. 30일 후 자동 영구 삭제 대상.</summary>
    Task<Result> DeleteAsync(string spaceKey, long pageId, string? actorId, CancellationToken ct = default);

    /// <summary>페이지를 다른 Space 또는 다른 부모 아래로 이동한다.</summary>
    Task<Result<Page>> MoveAsync(string spaceKey, long pageId, string targetSpaceKey, long? parentPageId, string? actorId, CancellationToken ct = default);

    /// <summary>페이지를 대상 Space로 복사한다. 제목에 [Copy] 접두사가 붙는다.</summary>
    Task<Result<Page>> CopyAsync(string spaceKey, long pageId, string targetSpaceKey, long? parentPageId, string? actorId, CancellationToken ct = default);

    /// <summary>Space에 속한 모든 페이지를 조회한다.</summary>
    Task<Result<List<Page>>> GetBySpaceKeyAsync(string spaceKey, CancellationToken ct = default);

    /// <summary>Space 내 특정 페이지를 ID로 조회한다.</summary>
    Task<Result<Page>> GetByIdAsync(string spaceKey, long pageId, CancellationToken ct = default);
}

/// <summary>페이지 CRUD, 이동, 복사 서비스 구현체.</summary>
public sealed class PageService(
    IRepository<Space> spaceRepo,
    IRepository<Page> pageRepo,
    IRepository<PageVersion> versionRepo,
    IUnitOfWork uow) : IPageService
{
    public async Task<Result<Page>> CreateAsync(string spaceKey, string title, string content, long? parentPageId, int position, string? actorId, CancellationToken ct)
    {
        var space = await spaceRepo.FirstOrDefaultAsync(new SpaceByKeySpec(spaceKey), ct);
        if (space is null) return Result.Failure<Page>(DomainErrors.Space.NotFound(spaceKey));

        var pageResult = Page.Create(space, parentPageId, title, content, position, actorId);
        if (pageResult.IsFailure) return Result.Failure<Page>(pageResult.Error);

        await pageRepo.AddAsync(pageResult.Value, ct);
        await uow.SaveChangesAsync(ct);
        return pageResult;
    }

    public async Task<Result<Page>> UpdateAsync(string spaceKey, long pageId, string title, string content, int? position, string? actorId, CancellationToken ct)
    {
        var space = await spaceRepo.FirstOrDefaultAsync(new SpaceByKeySpec(spaceKey), ct);
        if (space is null) return Result.Failure<Page>(DomainErrors.Space.NotFound(spaceKey));

        var page = await pageRepo.FirstOrDefaultAsync(new PageByIdSpec(pageId), ct);
        if (page is null) return Result.Failure<Page>(DomainErrors.Page.NotFound(pageId));
        if (page.SpaceId != space.Id) return Result.Failure<Page>(DomainErrors.Page.NotInSpace(pageId, spaceKey));

        // Save version snapshot before update
        var allVersions = await versionRepo.ListAsync(new PageVersionsByPageIdSpec(pageId), ct);
        var nextVersion = allVersions.Count == 0 ? 0 : allVersions.Max(v => v.Version);
        var snapshotResult = PageVersion.Create(pageId, nextVersion, page.Title, page.Content, null);
        if (snapshotResult.IsSuccess)
            await versionRepo.AddAsync(snapshotResult.Value, ct);

        var result = page.Update(title, content, position);
        if (result.IsFailure) return Result.Failure<Page>(result.Error);

        await uow.SaveChangesAsync(ct);
        return page;
    }

    public async Task<Result> DeleteAsync(string spaceKey, long pageId, string? actorId, CancellationToken ct)
    {
        var space = await spaceRepo.FirstOrDefaultAsync(new SpaceByKeySpec(spaceKey), ct);
        if (space is null) return Result.Failure(DomainErrors.Space.NotFound(spaceKey));

        var page = await pageRepo.FirstOrDefaultAsync(new PageByIdSpec(pageId), ct);
        if (page is null) return Result.Failure(DomainErrors.Page.NotFound(pageId));
        if (page.SpaceId != space.Id) return Result.Failure(DomainErrors.Page.NotInSpace(pageId, spaceKey));

        page.SoftDelete(spaceKey, actorId);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<Page>> MoveAsync(string spaceKey, long pageId, string targetSpaceKey, long? parentPageId, string? actorId, CancellationToken ct)
    {
        var space = await spaceRepo.FirstOrDefaultAsync(new SpaceByKeySpec(spaceKey), ct);
        if (space is null) return Result.Failure<Page>(DomainErrors.Space.NotFound(spaceKey));

        var page = await pageRepo.FirstOrDefaultAsync(new PageByIdSpec(pageId), ct);
        if (page is null) return Result.Failure<Page>(DomainErrors.Page.NotFound(pageId));
        if (page.SpaceId != space.Id) return Result.Failure<Page>(DomainErrors.Page.NotInSpace(pageId, spaceKey));

        var targetSpace = await spaceRepo.FirstOrDefaultAsync(new SpaceByKeySpec(targetSpaceKey), ct);
        if (targetSpace is null) return Result.Failure<Page>(DomainErrors.Space.NotFound(targetSpaceKey));

        Page? parent = null;
        if (parentPageId.HasValue)
        {
            parent = await pageRepo.FirstOrDefaultAsync(new PageByIdSpec(parentPageId.Value), ct);
            if (parent is null) return Result.Failure<Page>(DomainErrors.Page.ParentNotFound(parentPageId.Value));
        }

        var moveResult = page.MoveTo(targetSpace, parent, actorId);
        if (moveResult.IsFailure) return Result.Failure<Page>(moveResult.Error);

        await uow.SaveChangesAsync(ct);
        return page;
    }

    public async Task<Result<Page>> CopyAsync(string spaceKey, long pageId, string targetSpaceKey, long? parentPageId, string? actorId, CancellationToken ct)
    {
        var space = await spaceRepo.FirstOrDefaultAsync(new SpaceByKeySpec(spaceKey), ct);
        if (space is null) return Result.Failure<Page>(DomainErrors.Space.NotFound(spaceKey));

        var sourcePage = await pageRepo.FirstOrDefaultAsync(new PageByIdSpec(pageId), ct);
        if (sourcePage is null) return Result.Failure<Page>(DomainErrors.Page.NotFound(pageId));
        if (sourcePage.SpaceId != space.Id) return Result.Failure<Page>(DomainErrors.Page.NotInSpace(pageId, spaceKey));

        var targetSpace = await spaceRepo.FirstOrDefaultAsync(new SpaceByKeySpec(targetSpaceKey), ct);
        if (targetSpace is null) return Result.Failure<Page>(DomainErrors.Space.NotFound(targetSpaceKey));

        if (parentPageId.HasValue)
        {
            var parent = await pageRepo.FirstOrDefaultAsync(new PageByIdSpec(parentPageId.Value), ct);
            if (parent is null) return Result.Failure<Page>(DomainErrors.Page.ParentNotFound(parentPageId.Value));
            if (parent.SpaceId != targetSpace.Id)
                return Result.Failure<Page>(DomainErrors.Page.ParentNotInTargetSpace(parentPageId.Value, targetSpaceKey));
        }

        var copyResult = Page.Create(targetSpace, parentPageId, $"[Copy] {sourcePage.Title}", sourcePage.Content, sourcePage.Position, actorId);
        if (copyResult.IsFailure) return Result.Failure<Page>(copyResult.Error);

        await pageRepo.AddAsync(copyResult.Value, ct);
        await uow.SaveChangesAsync(ct);
        return copyResult;
    }

    public async Task<Result<List<Page>>> GetBySpaceKeyAsync(string spaceKey, CancellationToken ct)
    {
        var space = await spaceRepo.FirstOrDefaultAsync(new SpaceByKeySpec(spaceKey), ct);
        if (space is null) return Result.Failure<List<Page>>(DomainErrors.Space.NotFound(spaceKey));

        var pages = await pageRepo.ListAsync(new PagesBySpaceIdSpec(space.Id), ct);
        return pages;
    }

    public async Task<Result<Page>> GetByIdAsync(string spaceKey, long pageId, CancellationToken ct)
    {
        var space = await spaceRepo.FirstOrDefaultAsync(new SpaceByKeySpec(spaceKey), ct);
        if (space is null) return Result.Failure<Page>(DomainErrors.Space.NotFound(spaceKey));

        var page = await pageRepo.FirstOrDefaultAsync(new PageByIdSpec(pageId), ct);
        if (page is null) return Result.Failure<Page>(DomainErrors.Page.NotFound(pageId));
        if (page.SpaceId != space.Id) return Result.Failure<Page>(DomainErrors.Page.NotInSpace(pageId, spaceKey));
        return page;
    }
}
