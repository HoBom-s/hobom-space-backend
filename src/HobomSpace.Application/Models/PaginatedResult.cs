namespace HobomSpace.Application.Models;

/// <summary>offset/limit 기반 페이지네이션 결과.</summary>
/// <typeparam name="T">항목 타입.</typeparam>
public record PaginatedResult<T>(List<T> Items, int TotalCount, int Offset, int Limit)
{
    /// <summary>offset과 limit를 유효 범위로 보정한다.</summary>
    public static (int offset, int limit) Clamp(int offset, int limit, int maxLimit = 100)
    {
        return (Math.Max(0, offset), Math.Clamp(limit, 1, maxLimit));
    }
}
