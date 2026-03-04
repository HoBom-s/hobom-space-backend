namespace HobomSpace.Api.Contracts;

public record ApiResponse<T>(bool Success, string Message, DateTime Timestamp, T? Items);

public static class ApiResponse
{
    public static ApiResponse<T> Ok<T>(T items, string message = "OK") =>
        new(true, message, DateTime.UtcNow, items);

    public static ApiResponse<T> Created<T>(T items, string message = "Created") =>
        new(true, message, DateTime.UtcNow, items);

    public static ApiResponse<object?> Error(string message) =>
        new(false, message, DateTime.UtcNow, null);
}
