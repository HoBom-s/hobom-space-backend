using Ardalis.Specification;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Application.Specifications;

public sealed class PageVersionsByPageIdSpec : Specification<PageVersion>
{
    public PageVersionsByPageIdSpec(long pageId)
        => Query.Where(v => v.PageId == pageId).OrderByDescending(v => v.Version);

    public PageVersionsByPageIdSpec(long pageId, int offset, int limit)
    {
        Query.Where(v => v.PageId == pageId)
             .OrderByDescending(v => v.Version)
             .Skip(offset)
             .Take(limit);
    }
}

public sealed class PageVersionCountByPageIdSpec : Specification<PageVersion>
{
    public PageVersionCountByPageIdSpec(long pageId) => Query.Where(v => v.PageId == pageId);
}

public sealed class PageVersionByNumberSpec : Specification<PageVersion>, ISingleResultSpecification<PageVersion>
{
    public PageVersionByNumberSpec(long pageId, int version)
        => Query.Where(v => v.PageId == pageId && v.Version == version);
}
