using HobomAdmin.Application.Ports.Out;

namespace HobomAdmin.Application.Ports.In;

public interface IGetOutboxSummaryUseCase
{
    Task<OutboxStatusSummary> ExecuteAsync(CancellationToken ct = default);
}
