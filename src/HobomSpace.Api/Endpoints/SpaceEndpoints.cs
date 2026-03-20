using HobomSpace.Api.Contracts;
using HobomSpace.Api.Extensions;
using HobomSpace.Application.Services;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Api.Endpoints;

/// <summary>Space CRUD 엔드포인트.</summary>
public static class SpaceEndpoints
{
    /// <summary>Space 관련 엔드포인트를 매핑한다.</summary>
    public static RouteGroupBuilder MapSpaceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/spaces").WithTags("Spaces");

        group.MapPost("/", async (CreateSpaceRequest request, ISpaceService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request.Key, request.Name, request.Description, ct);
            return result.ToHttpResult(space =>
                Results.Created($"/api/v1/spaces/{space.Key}", ApiResponse.Created(ToResponse(space))));
        }).Produces<ApiResponse<SpaceResponse>>(StatusCodes.Status201Created);

        group.MapGet("/", async (ISpaceService service, CancellationToken ct, int offset = 0, int limit = 20) =>
        {
            var result = await service.GetAllAsync(offset, limit, ct);
            return Results.Ok(ApiResponse.Ok(new PaginatedResponse<SpaceResponse>(
                result.Items.Select(ToResponse).ToList(), result.TotalCount, result.Offset, result.Limit)));
        }).Produces<ApiResponse<PaginatedResponse<SpaceResponse>>>();

        group.MapGet("/{key}", async (string key, ISpaceService service, CancellationToken ct) =>
        {
            var result = await service.GetByKeyAsync(key, ct);
            return result.ToHttpResult(space => Results.Ok(ApiResponse.Ok(ToResponse(space))));
        }).Produces<ApiResponse<SpaceResponse>>();

        group.MapPut("/{key}", async (string key, UpdateSpaceRequest request, ISpaceService service, CancellationToken ct) =>
        {
            var result = await service.UpdateAsync(key, request.Name, request.Description, ct);
            return result.ToHttpResult(space => Results.Ok(ApiResponse.Ok(ToResponse(space))));
        }).Produces<ApiResponse<SpaceResponse>>();

        group.MapDelete("/{key}", async (string key, ISpaceService service, CancellationToken ct) =>
        {
            var result = await service.DeleteAsync(key, ct);
            return result.ToHttpResult(() => Results.Ok(ApiResponse.Ok<object?>(null, "Deleted")));
        }).Produces<ApiResponse<object>>();

        return group;
    }

    private static SpaceResponse ToResponse(Space s) =>
        new(s.Id, s.Key, s.Name, s.Description, s.CreatedAt, s.UpdatedAt);
}
