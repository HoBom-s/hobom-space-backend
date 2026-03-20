namespace HobomSpace.Api.Contracts;

/// <summary>에러 캡처 요청 DTO.</summary>
public record CaptureErrorRequest(string Message, string? StackTrace, string Screen, string ErrorType, string? UserAgent);

/// <summary>에러 이벤트 응답 DTO.</summary>
public record ErrorEventResponse(long Id, string Message, string? StackTrace, string Screen, string ErrorType, string? UserAgent, string? Nickname, DateTime CreatedAt);
