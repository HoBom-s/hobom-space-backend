namespace HobomSpace.Domain.Entities;

/// <summary>
/// 클라이언트에서 발생한 에러를 캡처하는 이벤트. 디버깅 및 모니터링에 활용된다.
/// </summary>
public sealed class ErrorEvent
{
    private static readonly HashSet<string> ValidErrorTypes = ["SERVER_RESPONSE", "CLIENT_LOGIC"];

    public long Id { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public string? StackTrace { get; private set; }

    /// <summary>에러가 발생한 화면 경로.</summary>
    public string Screen { get; private set; } = string.Empty;

    /// <summary>에러 유형: SERVER_RESPONSE 또는 CLIENT_LOGIC.</summary>
    public string ErrorType { get; private set; } = string.Empty;

    public string? UserAgent { get; private set; }
    public string? Nickname { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ErrorEvent() { }

    /// <summary>에러 이벤트를 생성한다. Message는 2000자, Screen은 500자로 잘린다.</summary>
    public static ErrorEvent Create(string message, string? stackTrace, string screen, string errorType, string? userAgent, string? nickname)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ArgumentException.ThrowIfNullOrWhiteSpace(screen);
        ArgumentException.ThrowIfNullOrWhiteSpace(errorType);

        if (!ValidErrorTypes.Contains(errorType))
            throw new ArgumentException($"Invalid error type: {errorType}. Must be one of: {string.Join(", ", ValidErrorTypes)}");

        return new ErrorEvent
        {
            Message = message.Length > 2000 ? message[..2000] : message,
            StackTrace = stackTrace,
            Screen = screen.Length > 500 ? screen[..500] : screen,
            ErrorType = errorType,
            UserAgent = userAgent,
            Nickname = nickname,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
