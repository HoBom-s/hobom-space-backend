using HobomAdmin.Domain.Entities;
using HobomAdmin.Domain.Enums;

namespace HobomAdmin.Application.Ports.Out;

public interface IOutboxReader
{
    Task<IReadOnlyList<OutboxEntry>> GetRecentAsync(int limit, CancellationToken ct = default);
    Task<OutboxStatusSummary> GetStatusSummaryAsync(CancellationToken ct = default);
}

public record OutboxStatusSummary(int Pending, int Sent, int Failed);
