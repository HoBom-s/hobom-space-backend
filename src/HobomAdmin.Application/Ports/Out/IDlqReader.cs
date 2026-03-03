using HobomAdmin.Domain.Entities;

namespace HobomAdmin.Application.Ports.Out;

public interface IDlqReader
{
    Task<IReadOnlyList<string>> GetKeysAsync(string prefix, CancellationToken ct = default);
    Task<DlqEntry?> GetByKeyAsync(string key, CancellationToken ct = default);
}
