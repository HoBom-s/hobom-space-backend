using HobomSpace.Application.Models;
using HobomSpace.Application.Ports;
using HobomSpace.Domain.Entities;
using HobomSpace.Domain.Exceptions;

namespace HobomSpace.Application.Services;

public interface IPageVersionService
{
    Task<PageVersion> SaveVersionAsync(long pageId, CancellationToken ct = default);
    Task<PaginatedResult<PageVersion>> GetHistoryAsync(long pageId, int offset, int limit, CancellationToken ct = default);
    Task<PageVersion> GetVersionAsync(long pageId, int version, CancellationToken ct = default);
    Task<Page> RestoreVersionAsync(long pageId, int version, CancellationToken ct = default);
}

public sealed class PageVersionService(
    IPageRepository pageRepo,
    IPageVersionRepository versionRepo,
    IUnitOfWork unitOfWork) : IPageVersionService
{
    public async Task<PageVersion> SaveVersionAsync(long pageId, CancellationToken ct = default)
    {
        var page = await GetPageAsyncOrThrow(pageId, ct);
        var history = await versionRepo.GetByPageIdAsync(pageId, ct);
        var nextVersion = history.Count == 0 ? 0 : history.Max(v => v.Version);

        var snapshot = PageVersion.Create(pageId, nextVersion, page.Title, page.Content, null);

        await versionRepo.AddAsync(snapshot, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return snapshot;
    }

    public async Task<PaginatedResult<PageVersion>> GetHistoryAsync(long pageId, int offset, int limit, CancellationToken ct = default)
    {
        var items = await versionRepo.GetByPageIdAsync(pageId, offset, limit, ct);
        var total = await versionRepo.CountByPageIdAsync(pageId, ct);
        return new PaginatedResult<PageVersion>(items, total, offset, limit);
    }

    public async Task<PageVersion> GetVersionAsync(long pageId, int version, CancellationToken ct = default)
        => await GetVersionAsyncOrThrow(pageId, version, ct);

    public async Task<Page> RestoreVersionAsync(long pageId, int version, CancellationToken ct = default)
    {
        var foundVersion = await GetVersionAsyncOrThrow(pageId, version, ct);
        var page = await GetPageAsyncOrThrow(pageId, ct);

        page.Update(foundVersion.Title, foundVersion.Content, null);
        await unitOfWork.SaveChangesAsync(ct);

        return page;
    }

    private async Task<PageVersion> GetVersionAsyncOrThrow(long pageId, int version, CancellationToken ct = default)
        => await versionRepo.GetByVersionAsync(pageId, version, ct)
            ?? throw new NotFoundException($"PageVersion {version} not found");

    private async Task<Page> GetPageAsyncOrThrow(long pageId, CancellationToken ct = default)
        => await pageRepo.GetByIdAsync(pageId, ct)
            ?? throw new NotFoundException($"Page {pageId} not found");
}
