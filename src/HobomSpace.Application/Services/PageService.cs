using HobomSpace.Application.Ports;
using HobomSpace.Domain.Entities;
using HobomSpace.Domain.Exceptions;

namespace HobomSpace.Application.Services;

public interface IPageService
{
    Task<Page> CreateAsync(string spaceKey, string title, string content, long? parentPageId, int position, CancellationToken ct = default);
    Task<List<Page>> GetBySpaceKeyAsync(string spaceKey, CancellationToken ct = default);
    Task<Page> GetByIdAsync(long pageId, CancellationToken ct = default);
    Task<Page> UpdateAsync(long pageId, string title, string content, int? position, CancellationToken ct = default);
    Task DeleteAsync(long pageId, CancellationToken ct = default);
}

public sealed class PageService(ISpaceRepository spaceRepo, IPageRepository pageRepo, IUnitOfWork uow) : IPageService
{
    public async Task<Page> CreateAsync(string spaceKey, string title, string content, long? parentPageId, int position, CancellationToken ct = default)
    {
        var space = await spaceRepo.GetByKeyAsync(spaceKey, ct)
                    ?? throw new NotFoundException($"Space '{spaceKey}' not found.");

        var page = Page.Create(space.Id, parentPageId, title, content, position);
        await pageRepo.AddAsync(page, ct);
        await uow.SaveChangesAsync(ct);
        return page;
    }

    public async Task<List<Page>> GetBySpaceKeyAsync(string spaceKey, CancellationToken ct = default)
    {
        var space = await spaceRepo.GetByKeyAsync(spaceKey, ct)
                    ?? throw new NotFoundException($"Space '{spaceKey}' not found.");

        return await pageRepo.GetBySpaceIdAsync(space.Id, ct);
    }

    public async Task<Page> GetByIdAsync(long pageId, CancellationToken ct = default)
        => await pageRepo.GetByIdAsync(pageId, ct)
           ?? throw new NotFoundException($"Page {pageId} not found.");

    public async Task<Page> UpdateAsync(long pageId, string title, string content, int? position, CancellationToken ct = default)
    {
        var page = await pageRepo.GetByIdAsync(pageId, ct)
                   ?? throw new NotFoundException($"Page {pageId} not found.");

        page.Update(title, content, position);
        await uow.SaveChangesAsync(ct);
        return page;
    }

    public async Task DeleteAsync(long pageId, CancellationToken ct = default)
    {
        var page = await pageRepo.GetByIdAsync(pageId, ct)
                   ?? throw new NotFoundException($"Page {pageId} not found.");

        pageRepo.Remove(page);
        await uow.SaveChangesAsync(ct);
    }
}
