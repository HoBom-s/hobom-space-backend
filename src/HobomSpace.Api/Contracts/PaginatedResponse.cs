namespace HobomSpace.Api.Contracts;

public record PaginatedResponse<T>(List<T> Items, int TotalCount, int Offset, int Limit);
