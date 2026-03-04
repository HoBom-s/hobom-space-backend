namespace HobomSpace.Application.Ports;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct = default);
}
