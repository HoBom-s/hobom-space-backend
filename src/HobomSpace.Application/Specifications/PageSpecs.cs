using Ardalis.Specification;
using HobomSpace.Application.Helpers;
using HobomSpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HobomSpace.Application.Specifications;

public sealed class PageByIdSpec : Specification<Page>, ISingleResultSpecification<Page>
{
    public PageByIdSpec(long id) => Query.Where(p => p.Id == id);
}

public sealed class PagesBySpaceIdSpec : Specification<Page>
{
    public PagesBySpaceIdSpec(long spaceId) => Query.Where(p => p.SpaceId == spaceId).OrderBy(p => p.Position);
}

public sealed class SearchPagesSpec : Specification<Page>
{
    public SearchPagesSpec(string query, int offset, int limit)
    {
        var pattern = $"%{LikeQueryHelper.EscapeLikePattern(query)}%";
        Query.Where(p => EF.Functions.ILike(p.Title, pattern) || EF.Functions.ILike(p.Content, pattern))
             .OrderByDescending(p => p.UpdatedAt)
             .Skip(offset)
             .Take(limit);
    }
}

public sealed class SearchPagesCountSpec : Specification<Page>
{
    public SearchPagesCountSpec(string query)
    {
        var pattern = $"%{LikeQueryHelper.EscapeLikePattern(query)}%";
        Query.Where(p => EF.Functions.ILike(p.Title, pattern) || EF.Functions.ILike(p.Content, pattern));
    }
}

public sealed class SearchPagesBySpaceIdSpec : Specification<Page>
{
    public SearchPagesBySpaceIdSpec(long spaceId, string query, int offset, int limit)
    {
        var pattern = $"%{LikeQueryHelper.EscapeLikePattern(query)}%";
        Query.Where(p => p.SpaceId == spaceId &&
                        (EF.Functions.ILike(p.Title, pattern) || EF.Functions.ILike(p.Content, pattern)))
             .OrderByDescending(p => p.UpdatedAt)
             .Skip(offset)
             .Take(limit);
    }
}

public sealed class SearchPagesBySpaceIdCountSpec : Specification<Page>
{
    public SearchPagesBySpaceIdCountSpec(long spaceId, string query)
    {
        var pattern = $"%{LikeQueryHelper.EscapeLikePattern(query)}%";
        Query.Where(p => p.SpaceId == spaceId &&
                        (EF.Functions.ILike(p.Title, pattern) || EF.Functions.ILike(p.Content, pattern)));
    }
}

public sealed class DeletedPageByIdSpec : Specification<Page>, ISingleResultSpecification<Page>
{
    public DeletedPageByIdSpec(long id) => Query.Where(p => p.Id == id && p.DeletedAt != null).IgnoreQueryFilters();
}

public sealed class DeletedPagesBySpaceIdSpec : Specification<Page>
{
    public DeletedPagesBySpaceIdSpec(long spaceId, int offset, int limit)
    {
        Query.Where(p => p.SpaceId == spaceId && p.DeletedAt != null)
             .IgnoreQueryFilters()
             .OrderByDescending(p => p.DeletedAt)
             .Skip(offset)
             .Take(limit);
    }
}

public sealed class DeletedPagesCountBySpaceIdSpec : Specification<Page>
{
    public DeletedPagesCountBySpaceIdSpec(long spaceId)
    {
        Query.Where(p => p.SpaceId == spaceId && p.DeletedAt != null).IgnoreQueryFilters();
    }
}

public sealed class PurgableDeletedPagesSpec : Specification<Page>
{
    public PurgableDeletedPagesSpec(DateTime cutoff)
    {
        Query.Where(p => p.DeletedAt != null && p.DeletedAt < cutoff).IgnoreQueryFilters();
    }
}
