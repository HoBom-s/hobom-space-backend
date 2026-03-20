using HobomSpace.Api.Contracts;
using HobomSpace.Api.Extensions;
using HobomSpace.Application.Services;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Api.Endpoints;

/// <summary>페이지 버전 이력, 복원, diff 엔드포인트.</summary>
public static class PageVersionEndpoints
{
    /// <summary>페이지 버전 관련 엔드포인트를 매핑한다.</summary>
    public static RouteGroupBuilder MapPageVersionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/spaces/{spaceKey}/pages/{pageId:long}/versions").WithTags("Page Versions");

        group.MapGet("/", async (string spaceKey, long pageId,
            IPageService pageService, IVersionService versionService, CancellationToken ct, int offset = 0, int limit = 20) =>
        {
            var pageCheck = await pageService.GetByIdAsync(spaceKey, pageId, ct);
            if (pageCheck.IsFailure) return pageCheck.ToHttpResult(_ => Results.Ok());

            var result = await versionService.GetHistoryAsync(pageId, offset, limit, ct);
            return Results.Ok(ApiResponse.Ok(new PaginatedResponse<PageVersionResponse>(
                result.Items.Select(ToResponse).ToList(), result.TotalCount, result.Offset, result.Limit)));
        }).Produces<ApiResponse<PaginatedResponse<PageVersionResponse>>>();

        group.MapGet("/{version:int}", async (string spaceKey, long pageId, int version,
            IPageService pageService, IVersionService versionService, CancellationToken ct) =>
        {
            var pageCheck = await pageService.GetByIdAsync(spaceKey, pageId, ct);
            if (pageCheck.IsFailure) return pageCheck.ToHttpResult(_ => Results.Ok());

            var result = await versionService.GetVersionAsync(pageId, version, ct);
            return result.ToHttpResult(v => Results.Ok(ApiResponse.Ok(ToResponse(v))));
        }).Produces<ApiResponse<PageVersionResponse>>();

        group.MapPost("/{version:int}/restore", async (string spaceKey, long pageId, int version,
            IVersionService versionService, CancellationToken ct) =>
        {
            var result = await versionService.RestoreVersionAsync(pageId, version, ct);
            return result.ToHttpResult(page => Results.Ok(ApiResponse.Ok(ToPageResponse(page))));
        }).Produces<ApiResponse<PageResponse>>();

        group.MapGet("/diff", async (string spaceKey, long pageId, int from, int to,
            IPageService pageService, IVersionService versionService, CancellationToken ct) =>
        {
            var pageCheck = await pageService.GetByIdAsync(spaceKey, pageId, ct);
            if (pageCheck.IsFailure) return pageCheck.ToHttpResult(_ => Results.Ok());

            var result = await versionService.DiffVersionsAsync(pageId, from, to, ct);
            return result.ToHttpResult(entries =>
                Results.Ok(ApiResponse.Ok(entries.Select(e =>
                    new DiffEntryResponse(e.LineNumber, e.Type.ToString(), e.Content)).ToList())));
        }).Produces<ApiResponse<List<DiffEntryResponse>>>();

        return group;
    }

    private static PageVersionResponse ToResponse(PageVersion v) =>
        new(v.Id, v.PageId, v.Version, v.Title, v.Content, v.EditedBy, v.CreatedAt);

    private static PageResponse ToPageResponse(Page p) =>
        new(p.Id, p.SpaceId, p.ParentPageId, p.Title, p.Content, p.Position, p.CreatedAt, p.UpdatedAt);
}
