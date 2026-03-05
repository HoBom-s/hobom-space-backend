using FluentAssertions;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Tests.Unit.Domain;

public class OutboxMessageTests
{
    [Fact]
    public void Create_WithValidArgs_ReturnsPendingMessage()
    {
        var outbox = OutboxMessage.Create("SPACE_EVENT", """{"entityType":"PAGE"}""");

        outbox.EventId.Should().NotBeNullOrEmpty();
        outbox.EventType.Should().Be("SPACE_EVENT");
        outbox.Status.Should().Be("PENDING");
        outbox.RetryCount.Should().Be(0);
        outbox.Version.Should().Be(1);
        outbox.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void MarkAsSent_SetsStatusAndTimestamp()
    {
        var outbox = OutboxMessage.Create("SPACE_EVENT", """{"entityType":"PAGE"}""");

        outbox.MarkAsSent();

        outbox.Status.Should().Be("SENT");
        outbox.SentAt.Should().NotBeNull();
        outbox.SentAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void MarkAsFailed_SetsStatusAndError()
    {
        var outbox = OutboxMessage.Create("SPACE_EVENT", """{"entityType":"PAGE"}""");

        outbox.MarkAsFailed("kafka timeout");

        outbox.Status.Should().Be("FAILED");
        outbox.FailedAt.Should().NotBeNull();
        outbox.LastError.Should().Be("kafka timeout");
        outbox.RetryCount.Should().Be(1);
    }

    [Fact]
    public void MarkAsFailed_CalledTwice_IncrementsRetryCount()
    {
        var outbox = OutboxMessage.Create("SPACE_EVENT", """{"entityType":"PAGE"}""");

        outbox.MarkAsFailed("error 1");
        outbox.MarkAsFailed("error 2");

        outbox.RetryCount.Should().Be(2);
        outbox.LastError.Should().Be("error 2");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidEventType_ThrowsArgumentException(string? eventType)
    {
        var act = () => OutboxMessage.Create(eventType!, """{"test":true}""");

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidPayload_ThrowsArgumentException(string? payload)
    {
        var act = () => OutboxMessage.Create("SPACE_EVENT", payload!);

        act.Should().Throw<ArgumentException>();
    }
}
