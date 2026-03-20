using HobomSpace.Api.Contracts;
using HobomSpace.Api.Extensions;
using HobomSpace.Application.Services;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Api.Endpoints;

/// <summary>페이지 검색 엔드포인트.</summary>
public static class SearchEndpoints
{
    /// <summary>검색 관련 엔드포인트를 매핑한다.</summary>
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
            return result.ToHttpResult(paginated =>
                Results.Ok(ApiResponse.Ok(new PaginatedResponse<SearchResult>(
                    paginated.Items.Select(ToResult).ToList(), paginated.TotalCount, paginated.Offset, paginated.Limit))));
        }).Produces<ApiResponse<PaginatedResponse<SearchResult>>>();

        return group;
    }

    private static SearchResult ToResult(Page p) =>
        new(p.Id, p.SpaceId, p.Title, p.UpdatedAt);
}
