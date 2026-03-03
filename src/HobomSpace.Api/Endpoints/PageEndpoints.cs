using HobomSpace.Application.Ports;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Api.Endpoints;

public static class PageEndpoints
{
    public record CreatePageRequest(string Title, string Content, long? ParentPageId, int Position = 0);
    public record UpdatePageRequest(string Title, string Content, int? Position);
    public record PageResponse(long Id, long SpaceId, long? ParentPageId, string Title, string Content, int Position, DateTime CreatedAt, DateTime UpdatedAt);
    public record PageTreeNode(long Id, string Title, int Position, List<PageTreeNode> Children);

    public static RouteGroupBuilder MapPageEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/spaces/{spaceKey}/pages").WithTags("Pages");

        group.MapPost("/", Create);
        group.MapGet("/", GetTree);
        group.MapGet("/{pageId:long}", GetById);
        group.MapPut("/{pageId:long}", Update);
        group.MapDelete("/{pageId:long}", Delete);

        return group;
    }

    private static async Task<IResult> Create(
        string spaceKey,
        CreatePageRequest request,
        ISpaceRepository spaceRepo,
        IPageRepository pageRepo,
        CancellationToken ct)
    {
        var space = await spaceRepo.GetByKeyAsync(spaceKey, ct);
        if (space is null)
            return Results.NotFound("Space not found.");

        var page = Page.Create(space.Id, request.ParentPageId, request.Title, request.Content, request.Position);
        await pageRepo.AddAsync(page, ct);
        await pageRepo.SaveChangesAsync(ct);

        return Results.Created($"/api/v1/spaces/{spaceKey}/pages/{page.Id}", ToResponse(page));
    }

    private static async Task<IResult> GetTree(
        string spaceKey,
        ISpaceRepository spaceRepo,
        IPageRepository pageRepo,
        CancellationToken ct)
    {
        var space = await spaceRepo.GetByKeyAsync(spaceKey, ct);
        if (space is null)
            return Results.NotFound("Space not found.");

        var pages = await pageRepo.GetBySpaceIdAsync(space.Id, ct);
        var tree = BuildTree(pages);

        return Results.Ok(tree);
    }

    private static async Task<IResult> GetById(
        string spaceKey,
        long pageId,
        IPageRepository pageRepo,
        CancellationToken ct)
    {
        var page = await pageRepo.GetByIdAsync(pageId, ct);
        return page is null ? Results.NotFound() : Results.Ok(ToResponse(page));
    }

    private static async Task<IResult> Update(
        string spaceKey,
        long pageId,
        UpdatePageRequest request,
        IPageRepository pageRepo,
        CancellationToken ct)
    {
        var page = await pageRepo.GetByIdAsync(pageId, ct);
        if (page is null)
            return Results.NotFound();

        page.Update(request.Title, request.Content, request.Position);
        await pageRepo.SaveChangesAsync(ct);

        return Results.Ok(ToResponse(page));
    }

    private static async Task<IResult> Delete(
        string spaceKey,
        long pageId,
        IPageRepository pageRepo,
        CancellationToken ct)
    {
        var page = await pageRepo.GetByIdAsync(pageId, ct);
        if (page is null)
            return Results.NotFound();

        pageRepo.Remove(page);
        await pageRepo.SaveChangesAsync(ct);

        return Results.NoContent();
    }

    private static List<PageTreeNode> BuildTree(List<Page> pages)
    {
        var lookup = pages.ToLookup(p => p.ParentPageId);

        List<PageTreeNode> Build(long? parentId) =>
            lookup[parentId]
                .OrderBy(p => p.Position)
                .Select(p => new PageTreeNode(p.Id, p.Title, p.Position, Build(p.Id)))
                .ToList();

        return Build(null);
    }

    private static PageResponse ToResponse(Page p) =>
        new(p.Id, p.SpaceId, p.ParentPageId, p.Title, p.Content, p.Position, p.CreatedAt, p.UpdatedAt);
}
