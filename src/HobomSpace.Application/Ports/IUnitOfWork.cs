namespace HobomSpace.Application.Ports;

/// <summary>트랜잭션 경계를 관리하는 Unit of Work 인터페이스.</summary>
public interface IUnitOfWork
{
    /// <summary>보류 중인 변경 사항을 영속화한다.</summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
