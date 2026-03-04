using HobomSpace.Api.Contracts;
using HobomSpace.Application.Services;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Api.Endpoints;

public static class PageEndpoints
{
    public static RouteGroupBuilder MapPageEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/spaces/{spaceKey}/pages").WithTags("Pages");

        group.MapPost("/", async (string spaceKey, CreatePageRequest request, IPageService service, CancellationToken ct) =>
        {
            var page = await service.CreateAsync(spaceKey, request.Title, request.Content, request.ParentPageId, request.Position, ct);
            return Results.Created($"/api/v1/spaces/{spaceKey}/pages/{page.Id}", ToResponse(page));
        }).Produces<PageResponse>(StatusCodes.Status201Created);

        group.MapGet("/", async (string spaceKey, IPageService service, CancellationToken ct) =>
            Results.Ok(BuildTree(await service.GetBySpaceKeyAsync(spaceKey, ct))))
            .Produces<List<PageTreeNode>>();

        group.MapGet("/{pageId:long}", async (string spaceKey, long pageId, IPageService service, CancellationToken ct) =>
            Results.Ok(ToResponse(await service.GetByIdAsync(spaceKey, pageId, ct))))
            .Produces<PageResponse>();

        group.MapPut("/{pageId:long}", async (string spaceKey, long pageId, UpdatePageRequest request, IPageService service, CancellationToken ct) =>
            Results.Ok(ToResponse(await service.UpdateAsync(spaceKey, pageId, request.Title, request.Content, request.Position, ct))))
            .Produces<PageResponse>();

        group.MapDelete("/{pageId:long}", async (string spaceKey, long pageId, IPageService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(spaceKey, pageId, ct);
            return Results.NoContent();
        }).Produces(StatusCodes.Status204NoContent);

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
