using HobomSpace.Api.Contracts;
using HobomSpace.Application.Services;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Api.Endpoints;

public static class PageVersionEndpoints
{
    public static RouteGroupBuilder MapPageVersionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/spaces/{spaceKey}/pages/{pageId:long}/versions").WithTags("Page Versions");

        group.MapGet("/", async (string spaceKey, long pageId,
            IPageService pageService, IPageVersionService service, CancellationToken ct, int offset = 0, int limit = 20) =>
        {
            await pageService.GetByIdAsync(spaceKey, pageId, ct);
            var result = await service.GetHistoryAsync(pageId, offset, limit, ct);
            return Results.Ok(ApiResponse.Ok(new PaginatedResponse<PageVersionResponse>(
                result.Items.Select(ToResponse).ToList(), result.TotalCount, result.Offset, result.Limit)));
        }).Produces<ApiResponse<PaginatedResponse<PageVersionResponse>>>();

        group.MapGet("/{version:int}", async (string spaceKey, long pageId, int version,
            IPageService pageService, IPageVersionService service, CancellationToken ct) =>
        {
            await pageService.GetByIdAsync(spaceKey, pageId, ct);
            return Results.Ok(ApiResponse.Ok(ToResponse(await service.GetVersionAsync(pageId, version, ct))));
        }).Produces<ApiResponse<PageVersionResponse>>();

        group.MapPost("/{version:int}/restore", async (string spaceKey, long pageId, int version,
            IPageService pageService, IPageVersionService service, CancellationToken ct) =>
        {
            await pageService.GetByIdAsync(spaceKey, pageId, ct);
            return Results.Ok(ApiResponse.Ok(ToPageResponse(await service.RestoreVersionAsync(pageId, version, ct))));
        }).Produces<ApiResponse<PageResponse>>();

        return group;
    }

    private static PageVersionResponse ToResponse(PageVersion v) =>
        new(v.Id, v.PageId, v.Version, v.Title, v.Content, v.EditedBy, v.CreatedAt);

    private static PageResponse ToPageResponse(Page p) =>
        new(p.Id, p.SpaceId, p.ParentPageId, p.Title, p.Content, p.Position, p.CreatedAt, p.UpdatedAt);
}
