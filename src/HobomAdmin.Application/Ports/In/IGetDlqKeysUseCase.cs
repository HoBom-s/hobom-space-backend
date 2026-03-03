namespace HobomAdmin.Application.Ports.In;

public interface IGetDlqKeysUseCase
{
    Task<IReadOnlyList<string>> ExecuteAsync(string prefix, CancellationToken ct = default);
}
