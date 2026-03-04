using HobomSpace.Application.Services;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Api.Endpoints;

public static class SpaceEndpoints
{
    public record CreateSpaceRequest(string Key, string Name, string? Description);
    public record UpdateSpaceRequest(string Name, string? Description);
    public record SpaceResponse(long Id, string Key, string Name, string? Description, DateTime CreatedAt, DateTime UpdatedAt);

    public static RouteGroupBuilder MapSpaceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/spaces").WithTags("Spaces");

        group.MapPost("/", async (CreateSpaceRequest request, ISpaceService service, CancellationToken ct) =>
        {
            var space = await service.CreateAsync(request.Key, request.Name, request.Description, ct);
            return Results.Created($"/api/v1/spaces/{space.Key}", ToResponse(space));
        });

        group.MapGet("/", async (ISpaceService service, CancellationToken ct) =>
            Results.Ok((await service.GetAllAsync(ct)).Select(ToResponse)));

        group.MapGet("/{key}", async (string key, ISpaceService service, CancellationToken ct) =>
            Results.Ok(ToResponse(await service.GetByKeyAsync(key, ct))));

        group.MapPut("/{key}", async (string key, UpdateSpaceRequest request, ISpaceService service, CancellationToken ct) =>
            Results.Ok(ToResponse(await service.UpdateAsync(key, request.Name, request.Description, ct))));

        group.MapDelete("/{key}", async (string key, ISpaceService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(key, ct);
            return Results.NoContent();
        });

        return group;
    }

    private static SpaceResponse ToResponse(Space s) =>
        new(s.Id, s.Key, s.Name, s.Description, s.CreatedAt, s.UpdatedAt);
}
