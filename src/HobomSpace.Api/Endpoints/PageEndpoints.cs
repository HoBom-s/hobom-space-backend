using HobomSpace.Api.Contracts;
using HobomSpace.Api.Extensions;
using HobomSpace.Application.Services;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Api.Endpoints;

/// <summary>페이지 CRUD, 이동, 복사 엔드포인트.</summary>
public static class PageEndpoints
{
    /// <summary>페이지 관련 엔드포인트를 매핑한다.</summary>
    public static RouteGroupBuilder MapPageEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/spaces/{spaceKey}/pages").WithTags("Pages");

        group.MapPost("/", async (string spaceKey, CreatePageRequest request, IPageService service, HttpContext context, CancellationToken ct) =>
        {
            var actorId = context.Request.Headers["X-User-Nickname"].FirstOrDefault();
            var result = await service.CreateAsync(spaceKey, request.Title, request.Content, request.ParentPageId, request.Position, actorId, ct);
            return result.ToHttpResult(page =>
                Results.Created($"/api/v1/spaces/{spaceKey}/pages/{page.Id}", ApiResponse.Created(ToResponse(page))));
        }).Produces<ApiResponse<PageResponse>>(StatusCodes.Status201Created);

        group.MapGet("/", async (string spaceKey, IPageService service, ILabelService labelService, CancellationToken ct) =>
        {
            var result = await service.GetBySpaceKeyAsync(spaceKey, ct);
            return await result.ToHttpResultAsync(async pages =>
            {
                var labelMap = await labelService.GetLabelsForPagesAsync(pages.Select(p => p.Id), ct);
                return Results.Ok(ApiResponse.Ok(BuildTree(pages, labelMap)));
            });
        }).Produces<ApiResponse<List<PageTreeNode>>>();

        group.MapGet("/{pageId:long}", async (string spaceKey, long pageId, IPageService service, ILabelService labelService, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(spaceKey, pageId, ct);
            return await result.ToHttpResultAsync(async page =>
            {
                var labelMap = await labelService.GetLabelsForPagesAsync([page.Id], ct);
                return Results.Ok(ApiResponse.Ok(ToResponse(page, labelMap)));
            });
        }).Produces<ApiResponse<PageResponse>>();

        group.MapPut("/{pageId:long}", async (string spaceKey, long pageId, UpdatePageRequest request, IPageService service, HttpContext context, CancellationToken ct) =>
        {
            var actorId = context.Request.Headers["X-User-Nickname"].FirstOrDefault();
            var result = await service.UpdateAsync(spaceKey, pageId, request.Title, request.Content, request.Position, actorId, ct);
            return result.ToHttpResult(page => Results.Ok(ApiResponse.Ok(ToResponse(page))));
        }).Produces<ApiResponse<PageResponse>>();

        group.MapDelete("/{pageId:long}", async (string spaceKey, long pageId, IPageService service, HttpContext context, CancellationToken ct) =>
        {
            var actorId = context.Request.Headers["X-User-Nickname"].FirstOrDefault();
            var result = await service.DeleteAsync(spaceKey, pageId, actorId, ct);
            return result.ToHttpResult(() => Results.Ok(ApiResponse.Ok<object?>(null, "Deleted")));
        }).Produces<ApiResponse<object>>();

        group.MapPatch("/{pageId:long}/move", async (string spaceKey, long pageId, MovePageRequest request, IPageService service, HttpContext context, CancellationToken ct) =>
        {
            var actorId = context.Request.Headers["X-User-Nickname"].FirstOrDefault();
            var result = await service.MoveAsync(spaceKey, pageId, request.TargetSpaceKey, request.ParentPageId, actorId, ct);
            return result.ToHttpResult(page => Results.Ok(ApiResponse.Ok(ToResponse(page))));
        }).Produces<ApiResponse<PageResponse>>();

        group.MapPost("/{pageId:long}/copy", async (string spaceKey, long pageId, CopyPageRequest request, IPageService service, HttpContext context, CancellationToken ct) =>
        {
            var actorId = context.Request.Headers["X-User-Nickname"].FirstOrDefault();
            var result = await service.CopyAsync(spaceKey, pageId, request.TargetSpaceKey, request.ParentPageId, actorId, ct);
            return result.ToHttpResult(copy =>
                Results.Created($"/api/v1/spaces/{request.TargetSpaceKey}/pages/{copy.Id}", ApiResponse.Created(ToResponse(copy))));
        }).Produces<ApiResponse<PageResponse>>(StatusCodes.Status201Created);

        return group;
    }

    private static List<PageTreeNode> BuildTree(List<Page> pages, Dictionary<long, List<Label>> labelMap)
    {
        var lookup = pages.ToLookup(p => p.ParentPageId);

        List<PageTreeNode> Build(long? parentId) =>
            lookup[parentId]
                .OrderBy(p => p.Position)
                .Select(p => new PageTreeNode(p.Id, p.Title, p.Position, ToSummaries(labelMap, p.Id), Build(p.Id)))
                .ToList();

        return Build(null);
    }

    private static PageResponse ToResponse(Page p) =>
        new(p.Id, p.SpaceId, p.ParentPageId, p.Title, p.Content, p.Position, p.CreatedAt, p.UpdatedAt);

    private static PageResponse ToResponse(Page p, Dictionary<long, List<Label>> labelMap) =>
        new(p.Id, p.SpaceId, p.ParentPageId, p.Title, p.Content, p.Position, p.CreatedAt, p.UpdatedAt, ToSummaries(labelMap, p.Id));

    private static List<LabelSummary> ToSummaries(Dictionary<long, List<Label>> labelMap, long pageId) =>
        labelMap.TryGetValue(pageId, out var labels)
            ? labels.OrderBy(l => l.Name).Select(l => new LabelSummary(l.Id, l.Name, l.Color)).ToList()
            : [];
}
