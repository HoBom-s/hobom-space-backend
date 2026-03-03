using HobomAdmin.Application.Ports.In;
using HobomAdmin.Application.Ports.Out;

namespace HobomAdmin.Application.UseCases;

public class GetDlqKeysUseCase(IDlqReader dlqReader) : IGetDlqKeysUseCase
{
    public Task<IReadOnlyList<string>> ExecuteAsync(string prefix, CancellationToken ct = default)
        => dlqReader.GetKeysAsync(prefix, ct);
}
