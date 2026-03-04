using HobomSpace.Application.Services;
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

        group.MapPost("/", async (string spaceKey, CreatePageRequest request, IPageService service, CancellationToken ct) =>
        {
            var page = await service.CreateAsync(spaceKey, request.Title, request.Content, request.ParentPageId, request.Position, ct);
            return Results.Created($"/api/v1/spaces/{spaceKey}/pages/{page.Id}", ToResponse(page));
        });

        group.MapGet("/", async (string spaceKey, IPageService service, CancellationToken ct) =>
            Results.Ok(BuildTree(await service.GetBySpaceKeyAsync(spaceKey, ct))));

        group.MapGet("/{pageId:long}", async (string spaceKey, long pageId, IPageService service, CancellationToken ct) =>
            Results.Ok(ToResponse(await service.GetByIdAsync(pageId, ct))));

        group.MapPut("/{pageId:long}", async (string spaceKey, long pageId, UpdatePageRequest request, IPageService service, CancellationToken ct) =>
            Results.Ok(ToResponse(await service.UpdateAsync(pageId, request.Title, request.Content, request.Position, ct))));

        group.MapDelete("/{pageId:long}", async (string spaceKey, long pageId, IPageService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(pageId, ct);
            return Results.NoContent();
        });

        return group;
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
