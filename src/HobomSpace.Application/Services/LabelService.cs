using HobomSpace.Application.Ports;
using HobomSpace.Application.Specifications;
using HobomSpace.Domain.Common;
using HobomSpace.Domain.Entities;
using HobomSpace.Domain.ValueObjects;

namespace HobomSpace.Application.Services;

/// <summary>라벨 CRUD 및 페이지-라벨 연결 연산을 정의한다.</summary>
public interface ILabelService
{
    /// <summary>Space에 새 라벨을 생성한다.</summary>
    Task<Result<Label>> CreateAsync(string spaceKey, string name, string color, CancellationToken ct = default);

    /// <summary>라벨의 이름과 색상을 수정한다.</summary>
    Task<Result<Label>> UpdateAsync(string spaceKey, long labelId, string name, string color, CancellationToken ct = default);

    /// <summary>라벨을 삭제한다.</summary>
    Task<Result> DeleteAsync(string spaceKey, long labelId, CancellationToken ct = default);

    /// <summary>페이지에 라벨을 부착한다. 이미 부착된 경우 실패한다.</summary>
    Task<Result<PageLabel>> AddToPageAsync(string spaceKey, long pageId, long labelId, CancellationToken ct = default);

    /// <summary>페이지에서 라벨을 제거한다.</summary>
    Task<Result> RemoveFromPageAsync(string spaceKey, long pageId, long labelId, CancellationToken ct = default);

    /// <summary>Space에 속한 모든 라벨을 조회한다.</summary>
    Task<Result<List<Label>>> GetBySpaceKeyAsync(string spaceKey, CancellationToken ct = default);

    /// <summary>특정 라벨이 부착된 페이지 목록을 조회한다.</summary>
    Task<Result<List<Page>>> GetPagesByLabelAsync(string spaceKey, long labelId, CancellationToken ct = default);
}

/// <summary>라벨 CRUD 및 페이지-라벨 연결 서비스 구현체.</summary>
public sealed class LabelService(
    IRepository<Space> spaceRepo,
    IRepository<Page> pageRepo,
    IRepository<Label> labelRepo,
    IRepository<PageLabel> pageLabelRepo,
    IUnitOfWork uow) : ILabelService
{
    public async Task<Result<Label>> CreateAsync(string spaceKey, string name, string color, CancellationToken ct)
    {
        var space = await spaceRepo.FirstOrDefaultAsync(new SpaceByKeySpec(spaceKey), ct);
        if (space is null) return Result.Failure<Label>(DomainErrors.Space.NotFound(spaceKey));

        var colorResult = HexColor.Create(color);
        if (colorResult.IsFailure) return Result.Failure<Label>(colorResult.Error);

        var labelResult = Label.Create(space.Id, name, colorResult.Value);
        if (labelResult.IsFailure) return Result.Failure<Label>(labelResult.Error);

        await labelRepo.AddAsync(labelResult.Value, ct);
        await uow.SaveChangesAsync(ct);
        return labelResult;
    }

    public async Task<Result<Label>> UpdateAsync(string spaceKey, long labelId, string name, string color, CancellationToken ct)
    {
        var space = await spaceRepo.FirstOrDefaultAsync(new SpaceByKeySpec(spaceKey), ct);
        if (space is null) return Result.Failure<Label>(DomainErrors.Space.NotFound(spaceKey));

        var label = await labelRepo.FirstOrDefaultAsync(new LabelByIdSpec(labelId), ct);
        if (label is null) return Result.Failure<Label>(DomainErrors.Label.NotFound(labelId));
        if (label.SpaceId != space.Id) return Result.Failure<Label>(DomainErrors.Label.NotInSpace(labelId));

        var colorResult = HexColor.Create(color);
        if (colorResult.IsFailure) return Result.Failure<Label>(colorResult.Error);

        var result = label.Update(name, colorResult.Value);
        if (result.IsFailure) return Result.Failure<Label>(result.Error);

        await uow.SaveChangesAsync(ct);
        return label;
    }

    public async Task<Result> DeleteAsync(string spaceKey, long labelId, CancellationToken ct)
    {
        var space = await spaceRepo.FirstOrDefaultAsync(new SpaceByKeySpec(spaceKey), ct);
        if (space is null) return Result.Failure(DomainErrors.Space.NotFound(spaceKey));

        var label = await labelRepo.FirstOrDefaultAsync(new LabelByIdSpec(labelId), ct);
        if (label is null) return Result.Failure(DomainErrors.Label.NotFound(labelId));
        if (label.SpaceId != space.Id) return Result.Failure(DomainErrors.Label.NotInSpace(labelId));

        await labelRepo.DeleteAsync(label, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<PageLabel>> AddToPageAsync(string spaceKey, long pageId, long labelId, CancellationToken ct)
    {
        var space = await spaceRepo.FirstOrDefaultAsync(new SpaceByKeySpec(spaceKey), ct);
        if (space is null) return Result.Failure<PageLabel>(DomainErrors.Space.NotFound(spaceKey));

        var page = await pageRepo.FirstOrDefaultAsync(new PageByIdSpec(pageId), ct);
        if (page is null) return Result.Failure<PageLabel>(DomainErrors.Page.NotFound(pageId));
        if (page.SpaceId != space.Id) return Result.Failure<PageLabel>(DomainErrors.Page.NotInSpace(pageId, spaceKey));

        var label = await labelRepo.FirstOrDefaultAsync(new LabelByIdSpec(labelId), ct);
        if (label is null) return Result.Failure<PageLabel>(DomainErrors.Label.NotFound(labelId));
        if (label.SpaceId != space.Id) return Result.Failure<PageLabel>(DomainErrors.Label.NotInSpace(labelId));

        var existing = await pageLabelRepo.FirstOrDefaultAsync(new PageLabelByIdsSpec(pageId, label.Id), ct);
        if (existing is not null) return Result.Failure<PageLabel>(DomainErrors.Label.AlreadyAssigned(pageId));

        var pageLabel = PageLabel.Create(pageId, label.Id);
        await pageLabelRepo.AddAsync(pageLabel, ct);
        await uow.SaveChangesAsync(ct);
        return pageLabel;
    }

    public async Task<Result> RemoveFromPageAsync(string spaceKey, long pageId, long labelId, CancellationToken ct)
    {
        var space = await spaceRepo.FirstOrDefaultAsync(new SpaceByKeySpec(spaceKey), ct);
        if (space is null) return Result.Failure(DomainErrors.Space.NotFound(spaceKey));

        var page = await pageRepo.FirstOrDefaultAsync(new PageByIdSpec(pageId), ct);
        if (page is null) return Result.Failure(DomainErrors.Page.NotFound(pageId));
        if (page.SpaceId != space.Id) return Result.Failure(DomainErrors.Page.NotInSpace(pageId, spaceKey));

        var pageLabel = await pageLabelRepo.FirstOrDefaultAsync(new PageLabelByIdsSpec(pageId, labelId), ct);
        if (pageLabel is null) return Result.Failure(DomainErrors.Label.NotAssigned(labelId, pageId));

        await pageLabelRepo.DeleteAsync(pageLabel, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<List<Label>>> GetBySpaceKeyAsync(string spaceKey, CancellationToken ct)
    {
        var space = await spaceRepo.FirstOrDefaultAsync(new SpaceByKeySpec(spaceKey), ct);
        if (space is null) return Result.Failure<List<Label>>(DomainErrors.Space.NotFound(spaceKey));

        var labels = await labelRepo.ListAsync(new LabelsBySpaceIdSpec(space.Id), ct);
        return labels;
    }

    public async Task<Result<List<Page>>> GetPagesByLabelAsync(string spaceKey, long labelId, CancellationToken ct)
    {
        var space = await spaceRepo.FirstOrDefaultAsync(new SpaceByKeySpec(spaceKey), ct);
        if (space is null) return Result.Failure<List<Page>>(DomainErrors.Space.NotFound(spaceKey));

        var label = await labelRepo.FirstOrDefaultAsync(new LabelByIdSpec(labelId), ct);
        if (label is null) return Result.Failure<List<Page>>(DomainErrors.Label.NotFound(labelId));
        if (label.SpaceId != space.Id) return Result.Failure<List<Page>>(DomainErrors.Label.NotInSpace(labelId));

        var pageLabels = await pageLabelRepo.ListAsync(new PageIdsByLabelIdSpec(labelId), ct);
        var pages = new List<Page>();
        foreach (var pl in pageLabels)
        {
            var page = await pageRepo.FirstOrDefaultAsync(new PageByIdSpec(pl.PageId), ct);
            if (page is not null)
                pages.Add(page);
        }
        return pages;
    }
}
