namespace HobomSpace.Application.Models;

/// <summary>page/size 기반 페이지네이션 결과.</summary>
/// <typeparam name="T">항목 타입.</typeparam>
public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int Size);
