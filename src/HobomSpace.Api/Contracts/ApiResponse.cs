namespace HobomSpace.Api.Contracts;

/// <summary>공통 API 응답 래퍼.</summary>
/// <typeparam name="T">응답 데이터 타입.</typeparam>
public record ApiResponse<T>(bool Success, string Message, DateTime Timestamp, T? Items);

/// <summary>API 응답 팩토리 메서드 모음.</summary>
public static class ApiResponse
{
    /// <summary>성공(200 OK) 응답을 생성한다.</summary>
    public static ApiResponse<T> Ok<T>(T items, string message = "OK") =>
        new(true, message, DateTime.UtcNow, items);

    /// <summary>생성(201 Created) 응답을 생성한다.</summary>
    public static ApiResponse<T> Created<T>(T items, string message = "Created") =>
        new(true, message, DateTime.UtcNow, items);

    /// <summary>에러 응답을 생성한다.</summary>
    public static ApiResponse<object?> Error(string message) =>
        new(false, message, DateTime.UtcNow, null);
}
