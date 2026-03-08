namespace HobomSpace.Api.Contracts;

public record CaptureErrorRequest(string Message, string? StackTrace, string Screen, string ErrorType, string? UserAgent);
public record ErrorEventResponse(long Id, string Message, string? StackTrace, string Screen, string ErrorType, string? UserAgent, string? Nickname, DateTime CreatedAt);
