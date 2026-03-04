namespace HobomSpace.Application.Models;

public record PaginatedResult<T>(List<T> Items, int TotalCount, int Offset, int Limit)
{
    public static (int offset, int limit) Clamp(int offset, int limit, int maxLimit = 100)
    {
        return (Math.Max(0, offset), Math.Clamp(limit, 1, maxLimit));
    }
}
