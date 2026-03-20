using Ardalis.Specification;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Application.Specifications;

public sealed class LabelByIdSpec : Specification<Label>, ISingleResultSpecification<Label>
{
    public LabelByIdSpec(long id) => Query.Where(l => l.Id == id);
}

public sealed class LabelsBySpaceIdSpec : Specification<Label>
{
    public LabelsBySpaceIdSpec(long spaceId) => Query.Where(l => l.SpaceId == spaceId).OrderBy(l => l.Name);
}

public sealed class PageLabelByIdsSpec : Specification<PageLabel>, ISingleResultSpecification<PageLabel>
{
    public PageLabelByIdsSpec(long pageId, long labelId)
        => Query.Where(pl => pl.PageId == pageId && pl.LabelId == labelId);
}

public sealed class PageIdsByLabelIdSpec : Specification<PageLabel>
{
    public PageIdsByLabelIdSpec(long labelId) => Query.Where(pl => pl.LabelId == labelId);
}

public sealed class PageLabelsByPageIdsSpec : Specification<PageLabel>
{
    public PageLabelsByPageIdsSpec(IEnumerable<long> pageIds)
        => Query.Where(pl => pageIds.Contains(pl.PageId));
}

public sealed class LabelsByIdsSpec : Specification<Label>
{
    public LabelsByIdsSpec(IEnumerable<long> ids)
        => Query.Where(l => ids.Contains(l.Id));
}
