using HobomSpace.Api.Contracts;
using HobomSpace.Application.Services;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Api.Endpoints;

public static class SpaceEndpoints
{
    public static RouteGroupBuilder MapSpaceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/spaces").WithTags("Spaces");

        group.MapPost("/", async (CreateSpaceRequest request, ISpaceService service, CancellationToken ct) =>
        {
            var space = await service.CreateAsync(request.Key, request.Name, request.Description, ct);
            return Results.Created($"/api/v1/spaces/{space.Key}", ToResponse(space));
        }).Produces<SpaceResponse>(StatusCodes.Status201Created);

        group.MapGet("/", async (ISpaceService service, CancellationToken ct, int offset = 0, int limit = 20) =>
        {
            var result = await service.GetAllAsync(offset, limit, ct);
            return Results.Ok(new PaginatedResponse<SpaceResponse>(
                result.Items.Select(ToResponse).ToList(), result.TotalCount, result.Offset, result.Limit));
        }).Produces<PaginatedResponse<SpaceResponse>>();

        group.MapGet("/{key}", async (string key, ISpaceService service, CancellationToken ct) =>
            Results.Ok(ToResponse(await service.GetByKeyAsync(key, ct))))
            .Produces<SpaceResponse>();

        group.MapPut("/{key}", async (string key, UpdateSpaceRequest request, ISpaceService service, CancellationToken ct) =>
            Results.Ok(ToResponse(await service.UpdateAsync(key, request.Name, request.Description, ct))))
            .Produces<SpaceResponse>();

        group.MapDelete("/{key}", async (string key, ISpaceService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(key, ct);
            return Results.NoContent();
        }).Produces(StatusCodes.Status204NoContent);

        return group;
    }

    private static SpaceResponse ToResponse(Space s) =>
        new(s.Id, s.Key, s.Name, s.Description, s.CreatedAt, s.UpdatedAt);
}
