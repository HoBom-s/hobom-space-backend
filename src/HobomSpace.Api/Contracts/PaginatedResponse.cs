namespace HobomSpace.Api.Contracts;

/// <summary>페이지네이션 응답 DTO.</summary>
/// <typeparam name="T">항목 타입.</typeparam>
public record PaginatedResponse<T>(List<T> Items, int TotalCount, int Offset, int Limit);
