using HobomSpace.Application.Services;

namespace HobomSpace.Api.BackgroundServices;

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
            var cleaner = scope.ServiceProvider.GetRequiredService<IOutboxCleaner>();
            await cleaner.CleanupAsync(stoppingToken);
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
