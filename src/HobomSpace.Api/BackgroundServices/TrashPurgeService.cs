using HobomSpace.Application.Ports;

namespace HobomSpace.Api.BackgroundServices;

/// <summary>매일 10:00 UTC에 30일 지난 삭제된 페이지를 영구 삭제하는 백그라운드 서비스.</summary>
public sealed class TrashPurgeService(IServiceScopeFactory scopeFactory, ILogger<TrashPurgeService> logger) : BackgroundService
{
    private static readonly TimeOnly ScheduledTime = new(10, 0);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = TimeUntilNext(ScheduledTime);
            logger.LogInformation("Trash purge scheduled in {Delay}", delay);
            await Task.Delay(delay, stoppingToken);

            using var scope = scopeFactory.CreateScope();
            var pageRepo = scope.ServiceProvider.GetRequiredService<IPageRepository>();
            var cutoff = DateTime.UtcNow.AddDays(-30);
            await pageRepo.PurgeDeletedOlderThanAsync(cutoff, 1000, stoppingToken);
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
