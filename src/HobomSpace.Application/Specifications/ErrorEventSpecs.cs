using Ardalis.Specification;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Application.Specifications;

public sealed class ErrorEventByIdSpec : Specification<ErrorEvent>, ISingleResultSpecification<ErrorEvent>
{
    public ErrorEventByIdSpec(long id) => Query.Where(e => e.Id == id);
}

public sealed class ErrorEventsPaginatedSpec : Specification<ErrorEvent>
{
    public ErrorEventsPaginatedSpec(int page, int size, string? errorType, string? screen)
    {
        var q = Query.OrderByDescending(e => e.CreatedAt);

        if (!string.IsNullOrEmpty(errorType))
            q.Where(e => e.ErrorType == errorType);
        if (!string.IsNullOrEmpty(screen))
            q.Where(e => e.Screen == screen);

        q.Skip(page * size).Take(size);
    }
}

public sealed class ErrorEventsCountSpec : Specification<ErrorEvent>
{
    public ErrorEventsCountSpec(string? errorType, string? screen)
    {
        if (!string.IsNullOrEmpty(errorType))
            Query.Where(e => e.ErrorType == errorType);
        if (!string.IsNullOrEmpty(screen))
            Query.Where(e => e.Screen == screen);
    }
}
