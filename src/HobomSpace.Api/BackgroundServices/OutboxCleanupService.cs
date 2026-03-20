using HobomSpace.Application.Ports;

namespace HobomSpace.Api.BackgroundServices;

/// <summary>매일 09:00 UTC에 오래된 Outbox 메시지를 정리하는 백그라운드 서비스.</summary>
public sealed class OutboxCleanupService(IServiceScopeFactory scopeFactory, ILogger<OutboxCleanupService> logger) : BackgroundService
{
    private static readonly TimeOnly ScheduledTime = new(9, 0);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = TimeUntilNext(ScheduledTime);
            logger.LogInformation("Outbox cleanup scheduled in {Delay}", delay);
            await Task.Delay(delay, stoppingToken);

            using var scope = scopeFactory.CreateScope();
            var outboxRepo = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
            var cutoff = DateTime.UtcNow.AddDays(-30);
            await outboxRepo.DeleteOlderThanAsync(cutoff, 1000, stoppingToken);
        }
    }

    private static TimeSpan TimeUntilNext(TimeOnly target)
    {
        var now = DateTime.UtcNow;
        var next = now.Date.Add(target.ToTimeSpan());
        if (next <= now) next = next.AddDays(1);
        return next - now;
    }
}
