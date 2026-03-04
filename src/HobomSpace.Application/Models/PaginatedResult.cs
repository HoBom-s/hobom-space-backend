namespace HobomSpace.Application.Models;

public record PaginatedResult<T>(List<T> Items, int TotalCount, int Offset, int Limit);
