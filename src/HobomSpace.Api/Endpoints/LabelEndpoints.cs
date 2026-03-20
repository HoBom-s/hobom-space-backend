using HobomSpace.Api.Contracts;
using HobomSpace.Api.Extensions;
using HobomSpace.Application.Services;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Api.Endpoints;

/// <summary>라벨 CRUD 및 페이지-라벨 연결 엔드포인트.</summary>
public static class LabelEndpoints
{
    /// <summary>라벨 관련 엔드포인트를 매핑한다.</summary>
    public static RouteGroupBuilder MapLabelEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/spaces/{spaceKey}").WithTags("Labels");

        group.MapPost("/labels", async (string spaceKey, CreateLabelRequest request, ILabelService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(spaceKey, request.Name, request.Color, ct);
            return result.ToHttpResult(label =>
                Results.Created($"/api/v1/spaces/{spaceKey}/labels/{label.Id}", ApiResponse.Created(ToResponse(label))));
        }).Produces<ApiResponse<LabelResponse>>(StatusCodes.Status201Created);

        group.MapGet("/labels", async (string spaceKey, ILabelService service, CancellationToken ct) =>
        {
            var result = await service.GetBySpaceKeyAsync(spaceKey, ct);
            return result.ToHttpResult(labels =>
                Results.Ok(ApiResponse.Ok(labels.Select(ToResponse).ToList())));
        }).Produces<ApiResponse<List<LabelResponse>>>();

        group.MapPut("/labels/{labelId:long}", async (string spaceKey, long labelId, UpdateLabelRequest request, ILabelService service, CancellationToken ct) =>
        {
            var result = await service.UpdateAsync(spaceKey, labelId, request.Name, request.Color, ct);
            return result.ToHttpResult(label => Results.Ok(ApiResponse.Ok(ToResponse(label))));
        }).Produces<ApiResponse<LabelResponse>>();

        group.MapDelete("/labels/{labelId:long}", async (string spaceKey, long labelId, ILabelService service, CancellationToken ct) =>
        {
            var result = await service.DeleteAsync(spaceKey, labelId, ct);
            return result.ToHttpResult(() => Results.Ok(ApiResponse.Ok<object?>(null, "Deleted")));
        }).Produces<ApiResponse<object>>();

        group.MapPost("/pages/{pageId:long}/labels", async (string spaceKey, long pageId, AddPageLabelRequest request, ILabelService service, CancellationToken ct) =>
        {
            var result = await service.AddToPageAsync(spaceKey, pageId, request.LabelId, ct);
            return result.ToHttpResult(_ =>
                Results.Created($"/api/v1/spaces/{spaceKey}/pages/{pageId}/labels", ApiResponse.Created<object?>(null)));
        }).Produces<ApiResponse<object>>(StatusCodes.Status201Created);

        group.MapDelete("/pages/{pageId:long}/labels/{labelId:long}", async (string spaceKey, long pageId, long labelId, ILabelService service, CancellationToken ct) =>
        {
            var result = await service.RemoveFromPageAsync(spaceKey, pageId, labelId, ct);
            return result.ToHttpResult(() => Results.Ok(ApiResponse.Ok<object?>(null, "Removed")));
        }).Produces<ApiResponse<object>>();

        group.MapGet("/labels/{labelId:long}/pages", async (string spaceKey, long labelId, ILabelService service, CancellationToken ct) =>
        {
            var result = await service.GetPagesByLabelAsync(spaceKey, labelId, ct);
            return result.ToHttpResult(pages =>
                Results.Ok(ApiResponse.Ok(pages.Select(p =>
                    new PageResponse(p.Id, p.SpaceId, p.ParentPageId, p.Title, p.Content, p.Position, p.CreatedAt, p.UpdatedAt)).ToList())));
        }).Produces<ApiResponse<List<PageResponse>>>();

        return group;
    }

    private static LabelResponse ToResponse(Label l) =>
        new(l.Id, l.SpaceId, l.Name, l.Color, l.CreatedAt);
}
