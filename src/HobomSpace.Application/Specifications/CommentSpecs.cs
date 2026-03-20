using Ardalis.Specification;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Application.Specifications;

public sealed class CommentByIdSpec : Specification<Comment>, ISingleResultSpecification<Comment>
{
    public CommentByIdSpec(long id) => Query.Where(c => c.Id == id);
}

public sealed class CommentsByPageIdSpec : Specification<Comment>
{
    public CommentsByPageIdSpec(long pageId, int offset, int limit)
    {
        Query.Where(c => c.PageId == pageId)
             .OrderByDescending(c => c.CreatedAt)
             .Skip(offset)
             .Take(limit);
    }
}

public sealed class CommentCountByPageIdSpec : Specification<Comment>
{
    public CommentCountByPageIdSpec(long pageId) => Query.Where(c => c.PageId == pageId);
}
