using HobomSpace.Api.Contracts;
using HobomSpace.Api.Extensions;
using HobomSpace.Application.Services;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Api.Endpoints;

/// <summary>휴지통(삭제된 페이지) 조회, 복원, 영구삭제 엔드포인트.</summary>
public static class TrashEndpoints
{
    /// <summary>휴지통 관련 엔드포인트를 매핑한다.</summary>
    public static RouteGroupBuilder MapTrashEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/spaces/{spaceKey}/trash").WithTags("Trash");

        group.MapGet("/", async (string spaceKey, ITrashService service, CancellationToken ct, int offset = 0, int limit = 20) =>
        {
            var result = await service.GetDeletedPagesAsync(spaceKey, offset, limit, ct);
            return result.ToHttpResult(paginated =>
                Results.Ok(ApiResponse.Ok(new PaginatedResponse<TrashPageResponse>(
                    paginated.Items.Select(ToResponse).ToList(), paginated.TotalCount, paginated.Offset, paginated.Limit))));
        }).Produces<ApiResponse<PaginatedResponse<TrashPageResponse>>>();

        group.MapPost("/{pageId:long}/restore", async (string spaceKey, long pageId, ITrashService service, CancellationToken ct) =>
        {
            var result = await service.RestoreAsync(spaceKey, pageId, ct);
            return result.ToHttpResult(page => Results.Ok(ApiResponse.Ok(ToResponse(page))));
        }).Produces<ApiResponse<TrashPageResponse>>();

        group.MapDelete("/{pageId:long}", async (string spaceKey, long pageId, ITrashService service, CancellationToken ct) =>
        {
            var result = await service.PermanentDeleteAsync(spaceKey, pageId, ct);
            return result.ToHttpResult(() => Results.Ok(ApiResponse.Ok<object?>(null, "Permanently deleted")));
        }).Produces<ApiResponse<object>>();

        return group;
    }

    private static TrashPageResponse ToResponse(Page p) =>
        new(p.Id, p.SpaceId, p.ParentPageId, p.Title, p.Position, p.CreatedAt, p.DeletedAt, p.DeletedBy);
}
