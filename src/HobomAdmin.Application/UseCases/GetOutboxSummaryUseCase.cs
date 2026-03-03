using HobomAdmin.Application.Ports.In;
using HobomAdmin.Application.Ports.Out;

namespace HobomAdmin.Application.UseCases;

public class GetOutboxSummaryUseCase(IOutboxReader outboxReader) : IGetOutboxSummaryUseCase
{
    public Task<OutboxStatusSummary> ExecuteAsync(CancellationToken ct = default)
        => outboxReader.GetStatusSummaryAsync(ct);
}
