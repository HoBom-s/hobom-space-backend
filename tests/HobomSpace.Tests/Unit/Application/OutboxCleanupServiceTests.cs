using FluentAssertions;
using HobomSpace.Application.Ports;
using HobomSpace.Application.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HobomSpace.Tests.Unit.Application;

public class OutboxCleanerTests
{
    private readonly IOutboxRepository _outboxRepo = Substitute.For<IOutboxRepository>();
    private readonly OutboxCleaner _sut;

    public OutboxCleanerTests()
    {
        _sut = new OutboxCleaner(_outboxRepo, Substitute.For<ILogger<OutboxCleaner>>());
    }

    [Fact]
    public async Task CleanupAsync_DeletesInBatchesUntilDone()
    {
        _outboxRepo.DeleteOlderThanAsync(Arg.Any<DateTime>(), 100, Arg.Any<CancellationToken>())
            .Returns(100, 100, 42);

        await _sut.CleanupAsync();

        await _outboxRepo.Received(3).DeleteOlderThanAsync(Arg.Any<DateTime>(), 100, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CleanupAsync_StopsWhenBatchReturnedLessThanBatchSize()
    {
        _outboxRepo.DeleteOlderThanAsync(Arg.Any<DateTime>(), 100, Arg.Any<CancellationToken>())
            .Returns(50);

        await _sut.CleanupAsync();

        await _outboxRepo.Received(1).DeleteOlderThanAsync(Arg.Any<DateTime>(), 100, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CleanupAsync_StopsWhenNothingToDelete()
    {
        _outboxRepo.DeleteOlderThanAsync(Arg.Any<DateTime>(), 100, Arg.Any<CancellationToken>())
            .Returns(0);

        await _sut.CleanupAsync();

        await _outboxRepo.Received(1).DeleteOlderThanAsync(Arg.Any<DateTime>(), 100, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CleanupAsync_StopsOnCancellation()
    {
        var cts = new CancellationTokenSource();
        _outboxRepo.DeleteOlderThanAsync(Arg.Any<DateTime>(), 100, Arg.Any<CancellationToken>())
            .Returns(100)
            .AndDoes(_ => cts.Cancel());

        await _sut.CleanupAsync(cts.Token);

        await _outboxRepo.Received(1).DeleteOlderThanAsync(Arg.Any<DateTime>(), 100, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CleanupAsync_HandlesExceptionGracefully()
    {
        _outboxRepo.DeleteOlderThanAsync(Arg.Any<DateTime>(), 100, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("DB connection failed"));

        var act = () => _sut.CleanupAsync();

        await act.Should().NotThrowAsync();
    }
}
