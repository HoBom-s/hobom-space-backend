using HobomSpace.Application.Ports;
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

        group.MapPost("/", Create);
        group.MapGet("/", GetAll);
        group.MapGet("/{key}", GetByKey);
        group.MapPut("/{key}", Update);
        group.MapDelete("/{key}", Delete);

        return group;
    }

    private static async Task<IResult> Create(CreateSpaceRequest request, ISpaceRepository repo, CancellationToken ct)
    {
        var existing = await repo.GetByKeyAsync(request.Key, ct);
        if (existing is not null)
            return Results.Conflict($"Space with key '{request.Key.ToUpperInvariant()}' already exists.");

        var space = Space.Create(request.Key, request.Name, request.Description);
        await repo.AddAsync(space, ct);
        await repo.SaveChangesAsync(ct);

        return Results.Created($"/api/v1/spaces/{space.Key}", ToResponse(space));
    }

    private static async Task<IResult> GetAll(ISpaceRepository repo, CancellationToken ct)
    {
        var spaces = await repo.GetAllAsync(ct);
        return Results.Ok(spaces.Select(ToResponse));
    }

    private static async Task<IResult> GetByKey(string key, ISpaceRepository repo, CancellationToken ct)
    {
        var space = await repo.GetByKeyAsync(key, ct);
        return space is null ? Results.NotFound() : Results.Ok(ToResponse(space));
    }

    private static async Task<IResult> Update(string key, UpdateSpaceRequest request, ISpaceRepository repo, CancellationToken ct)
    {
        var space = await repo.GetByKeyAsync(key, ct);
        if (space is null)
            return Results.NotFound();

        space.Update(request.Name, request.Description);
        await repo.SaveChangesAsync(ct);

        return Results.Ok(ToResponse(space));
    }

    private static async Task<IResult> Delete(string key, ISpaceRepository repo, CancellationToken ct)
    {
        var space = await repo.GetByKeyAsync(key, ct);
        if (space is null)
            return Results.NotFound();

        repo.Remove(space);
        await repo.SaveChangesAsync(ct);

        return Results.NoContent();
    }

    private static SpaceResponse ToResponse(Space s) =>
        new(s.Id, s.Key, s.Name, s.Description, s.CreatedAt, s.UpdatedAt);
}
