using HobomSpace.Api.Contracts;
using HobomSpace.Application.Services;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Api.Endpoints;

public static class PageEndpoints
{
    public static RouteGroupBuilder MapPageEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/spaces/{spaceKey}/pages").WithTags("Pages");

        group.MapPost("/", async (string spaceKey, CreatePageRequest request, IPageService service, HttpContext context, CancellationToken ct) =>
        {
            var actorId = context.Request.Headers["X-User-Nickname"].FirstOrDefault();
            var page = await service.CreateAsync(spaceKey, request.Title, request.Content, request.ParentPageId, request.Position, actorId, ct);
            return Results.Created($"/api/v1/spaces/{spaceKey}/pages/{page.Id}", ApiResponse.Created(ToResponse(page)));
        }).Produces<ApiResponse<PageResponse>>(StatusCodes.Status201Created);

        group.MapGet("/", async (string spaceKey, IPageService service, CancellationToken ct) =>
            Results.Ok(ApiResponse.Ok(BuildTree(await service.GetBySpaceKeyAsync(spaceKey, ct)))))
            .Produces<ApiResponse<List<PageTreeNode>>>();

        group.MapGet("/{pageId:long}", async (string spaceKey, long pageId, IPageService service, CancellationToken ct) =>
            Results.Ok(ApiResponse.Ok(ToResponse(await service.GetByIdAsync(spaceKey, pageId, ct)))))
            .Produces<ApiResponse<PageResponse>>();

        group.MapPut("/{pageId:long}", async (string spaceKey, long pageId, UpdatePageRequest request, IPageService service, HttpContext context, CancellationToken ct) =>
        {
            var actorId = context.Request.Headers["X-User-Nickname"].FirstOrDefault();
            return Results.Ok(ApiResponse.Ok(ToResponse(await service.UpdateAsync(spaceKey, pageId, request.Title, request.Content, request.Position, actorId, ct))));
        }).Produces<ApiResponse<PageResponse>>();

        group.MapDelete("/{pageId:long}", async (string spaceKey, long pageId, IPageService service, HttpContext context, CancellationToken ct) =>
        {
            var actorId = context.Request.Headers["X-User-Nickname"].FirstOrDefault();
            await service.DeleteAsync(spaceKey, pageId, actorId, ct);
            return Results.Ok(ApiResponse.Ok<object?>(null, "Deleted"));
        }).Produces<ApiResponse<object>>();

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
