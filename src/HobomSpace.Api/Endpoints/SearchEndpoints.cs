using HobomSpace.Api.Contracts;
using HobomSpace.Application.Services;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Api.Endpoints;

public static class SearchEndpoints
{
    public static RouteGroupBuilder MapSearchEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/search").WithTags("Search");

        group.MapGet("/", async (string q, ISearchService service, CancellationToken ct, int offset = 0, int limit = 20) =>
        {
            var result = await service.SearchPagesAsync(q, offset, limit, ct);
            return Results.Ok(ApiResponse.Ok(new PaginatedResponse<SearchResult>(
                result.Items.Select(ToResult).ToList(), result.TotalCount, result.Offset, result.Limit)));
        }).Produces<ApiResponse<PaginatedResponse<SearchResult>>>();

        group.MapGet("/spaces/{spaceKey}", async (string spaceKey, string q, ISearchService service, CancellationToken ct, int offset = 0, int limit = 20) =>
        {
            var result = await service.SearchPagesInSpaceAsync(spaceKey, q, offset, limit, ct);
            return Results.Ok(ApiResponse.Ok(new PaginatedResponse<SearchResult>(
                result.Items.Select(ToResult).ToList(), result.TotalCount, result.Offset, result.Limit)));
        }).Produces<ApiResponse<PaginatedResponse<SearchResult>>>();

        return group;
    }

    private static SearchResult ToResult(Page p) =>
        new(p.Id, p.SpaceId, p.Title, p.UpdatedAt);
}
