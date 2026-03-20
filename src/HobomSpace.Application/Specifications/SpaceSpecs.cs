using Ardalis.Specification;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Application.Specifications;

public sealed class SpaceByKeySpec : Specification<Space>, ISingleResultSpecification<Space>
{
    public SpaceByKeySpec(string key) => Query.Where(s => s.Key == key);
}

public sealed class SpacesPaginatedSpec : Specification<Space>
{
    public SpacesPaginatedSpec(int offset, int limit)
    {
        Query.OrderByDescending(s => s.CreatedAt)
             .Skip(offset)
             .Take(limit);
    }
}
