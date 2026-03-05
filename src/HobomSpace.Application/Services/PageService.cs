using System.Text.Json;
using HobomSpace.Application.Ports;
using HobomSpace.Domain.Entities;
using HobomSpace.Domain.Exceptions;

namespace HobomSpace.Application.Services;

public interface IPageService
{
    Task<Page> CreateAsync(string spaceKey, string title, string content, long? parentPageId, int position, CancellationToken ct = default);
    Task<List<Page>> GetBySpaceKeyAsync(string spaceKey, CancellationToken ct = default);
    Task<Page> GetByIdAsync(string spaceKey, long pageId, CancellationToken ct = default);
    Task<Page> UpdateAsync(string spaceKey, long pageId, string title, string content, int? position, CancellationToken ct = default);
    Task DeleteAsync(string spaceKey, long pageId, CancellationToken ct = default);
}

public sealed class PageService(ISpaceRepository spaceRepo, IPageRepository pageRepo, IPageVersionService versionService, IOutboxRepository outboxRepo, IUnitOfWork uow) : IPageService
{
    public async Task<Page> CreateAsync(string spaceKey, string title, string content, long? parentPageId, int position, CancellationToken ct = default)
    {
        var space = await spaceRepo.GetByKeyAsync(spaceKey, ct)
                    ?? throw new NotFoundException($"Space '{spaceKey}' not found.");

        var page = Page.Create(space.Id, parentPageId, title, content, position);
        await pageRepo.AddAsync(page, ct);
        await outboxRepo.AddAsync(OutboxMessage.Create("SPACE_EVENT",
            JsonSerializer.Serialize(new { entityType = "PAGE", action = "CREATED", spaceKey, pageId = page.Id, title })), ct);
        await uow.SaveChangesAsync(ct);
        return page;
    }

    public async Task<List<Page>> GetBySpaceKeyAsync(string spaceKey, CancellationToken ct = default)
    {
        var space = await spaceRepo.GetByKeyAsync(spaceKey, ct)
                    ?? throw new NotFoundException($"Space '{spaceKey}' not found.");

        return await pageRepo.GetBySpaceIdAsync(space.Id, ct);
    }

    public async Task<Page> GetByIdAsync(string spaceKey, long pageId, CancellationToken ct = default)
    {
        var space = await spaceRepo.GetByKeyAsync(spaceKey, ct)
                    ?? throw new NotFoundException($"Space '{spaceKey}' not found.");
        var page = await pageRepo.GetByIdAsync(pageId, ct)
                   ?? throw new NotFoundException($"Page {pageId} not found.");
        if (page.SpaceId != space.Id)
            throw new NotFoundException($"Page {pageId} not found in space '{spaceKey}'.");
        return page;
    }

    public async Task<Page> UpdateAsync(string spaceKey, long pageId, string title, string content, int? position, CancellationToken ct = default)
    {
        var page = await GetByIdAsync(spaceKey, pageId, ct);
        await versionService.SaveVersionAsync(pageId, ct);
        page.Update(title, content, position);
        await outboxRepo.AddAsync(OutboxMessage.Create("SPACE_EVENT",
            JsonSerializer.Serialize(new { entityType = "PAGE", action = "UPDATED", spaceKey, pageId, title })), ct);
        await uow.SaveChangesAsync(ct);
        return page;
    }

    public async Task DeleteAsync(string spaceKey, long pageId, CancellationToken ct = default)
    {
        var page = await GetByIdAsync(spaceKey, pageId, ct);
        await outboxRepo.AddAsync(OutboxMessage.Create("SPACE_EVENT",
            JsonSerializer.Serialize(new { entityType = "PAGE", action = "DELETED", spaceKey, pageId, title = page.Title })), ct);
        pageRepo.Remove(page);
        await uow.SaveChangesAsync(ct);
    }
}
