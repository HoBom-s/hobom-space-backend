using FluentAssertions;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Tests.Unit.Domain;

public class ErrorEventTests
{
    [Fact]
    public void Create_WithValidArgs_ReturnsErrorEvent()
    {
        var errorEvent = ErrorEvent.Create("NullRef", "at Main()", "/home", "CLIENT_LOGIC", "Mozilla/5.0", "foxmon");

        errorEvent.Message.Should().Be("NullRef");
        errorEvent.StackTrace.Should().Be("at Main()");
        errorEvent.Screen.Should().Be("/home");
        errorEvent.ErrorType.Should().Be("CLIENT_LOGIC");
        errorEvent.UserAgent.Should().Be("Mozilla/5.0");
        errorEvent.Nickname.Should().Be("foxmon");
        errorEvent.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithNullOptionalFields_SetsNull()
    {
        var errorEvent = ErrorEvent.Create("error", null, "/page", "SERVER_RESPONSE", null, null);

        errorEvent.StackTrace.Should().BeNull();
        errorEvent.UserAgent.Should().BeNull();
        errorEvent.Nickname.Should().BeNull();
    }

    [Theory]
    [InlineData("SERVER_RESPONSE")]
    [InlineData("CLIENT_LOGIC")]
    public void Create_WithValidErrorType_Succeeds(string errorType)
    {
        var errorEvent = ErrorEvent.Create("msg", null, "/screen", errorType, null, null);

        errorEvent.ErrorType.Should().Be(errorType);
    }

    [Theory]
    [InlineData("INVALID")]
    [InlineData("server_response")]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidErrorType_ThrowsArgumentException(string errorType)
    {
        var act = () => ErrorEvent.Create("msg", null, "/screen", errorType, null, null);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidMessage_ThrowsArgumentException(string? message)
    {
        var act = () => ErrorEvent.Create(message!, null, "/screen", "CLIENT_LOGIC", null, null);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidScreen_ThrowsArgumentException(string? screen)
    {
        var act = () => ErrorEvent.Create("msg", null, screen!, "CLIENT_LOGIC", null, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithLongMessage_TruncatesTo2000()
    {
        var longMessage = new string('x', 3000);

        var errorEvent = ErrorEvent.Create(longMessage, null, "/screen", "CLIENT_LOGIC", null, null);

        errorEvent.Message.Should().HaveLength(2000);
    }

    [Fact]
    public void Create_WithLongScreen_TruncatesTo500()
    {
        var longScreen = "/" + new string('x', 600);

        var errorEvent = ErrorEvent.Create("msg", null, longScreen, "CLIENT_LOGIC", null, null);

        errorEvent.Screen.Should().HaveLength(500);
    }
}
