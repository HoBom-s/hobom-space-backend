namespace HobomSpace.Domain.Entities;

public sealed class ErrorEvent
{
    private static readonly HashSet<string> ValidErrorTypes = ["SERVER_RESPONSE", "CLIENT_LOGIC"];

    public long Id { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public string? StackTrace { get; private set; }
    public string Screen { get; private set; } = string.Empty;
    public string ErrorType { get; private set; } = string.Empty;
    public string? UserAgent { get; private set; }
    public string? Nickname { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ErrorEvent() { }

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
