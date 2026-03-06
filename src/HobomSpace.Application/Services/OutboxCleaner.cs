using HobomSpace.Application.Ports;
using Microsoft.Extensions.Logging;

namespace HobomSpace.Application.Services;

public interface IOutboxCleaner
{
    Task CleanupAsync(CancellationToken ct = default);
}

public sealed class OutboxCleaner(IOutboxRepository outboxRepo, ILogger<OutboxCleaner> logger) : IOutboxCleaner
{
    private const int RetentionDays = 30;
    private const int BatchSize = 100;

    public async Task CleanupAsync(CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-RetentionDays);
        var totalDeleted = 0;

        try
        {
            int deleted;
            do
            {
                deleted = await outboxRepo.DeleteOlderThanAsync(cutoff, BatchSize, ct);
                totalDeleted += deleted;
            } while (deleted == BatchSize && !ct.IsCancellationRequested);

            logger.LogInformation("Outbox cleanup completed: {Count} messages deleted (older than {Cutoff:yyyy-MM-dd})", totalDeleted, cutoff);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Outbox cleanup failed after deleting {Count} messages", totalDeleted);
        }
    }
}
