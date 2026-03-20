using HobomSpace.Application.Models;
using HobomSpace.Application.Ports;
using HobomSpace.Application.Specifications;
using HobomSpace.Domain.Common;
using HobomSpace.Domain.Entities;
using HobomSpace.Domain.ValueObjects;

namespace HobomSpace.Application.Services;

/// <summary>Space 도메인의 CRUD 연산을 정의한다.</summary>
public interface ISpaceService
{
    /// <summary>새 Space를 생성한다. Key는 대문자로 변환되며 중복 시 실패한다.</summary>
    Task<Result<Space>> CreateAsync(string key, string name, string? description, CancellationToken ct = default);

    /// <summary>기존 Space의 이름과 설명을 수정한다.</summary>
    Task<Result<Space>> UpdateAsync(string key, string name, string? description, CancellationToken ct = default);

    /// <summary>Space를 삭제한다.</summary>
    Task<Result> DeleteAsync(string key, CancellationToken ct = default);

    /// <summary>전체 Space 목록을 페이지네이션하여 조회한다.</summary>
    Task<PaginatedResult<Space>> GetAllAsync(int offset, int limit, CancellationToken ct = default);

    /// <summary>Key로 Space를 조회한다.</summary>
    Task<Result<Space>> GetByKeyAsync(string key, CancellationToken ct = default);
}

/// <summary>Space CRUD 서비스 구현체.</summary>
public sealed class SpaceService(IRepository<Space> spaceRepo, IUnitOfWork uow) : ISpaceService
{
    /// <inheritdoc />
    public async Task<Result<Space>> CreateAsync(string key, string name, string? description, CancellationToken ct)
    {
        var keyResult = SpaceKey.Create(key);
        if (keyResult.IsFailure) return Result.Failure<Space>(keyResult.Error);

        var existing = await spaceRepo.FirstOrDefaultAsync(new SpaceByKeySpec(keyResult.Value), ct);
        if (existing is not null) return Result.Failure<Space>(DomainErrors.Space.AlreadyExists(key));

        var spaceResult = Space.Create(keyResult.Value, name, description);
        if (spaceResult.IsFailure) return Result.Failure<Space>(spaceResult.Error);

        await spaceRepo.AddAsync(spaceResult.Value, ct);
        await uow.SaveChangesAsync(ct);
        return spaceResult;
    }

    /// <inheritdoc />
    public async Task<Result<Space>> UpdateAsync(string key, string name, string? description, CancellationToken ct)
    {
        var space = await spaceRepo.FirstOrDefaultAsync(new SpaceByKeySpec(key), ct);
        if (space is null) return Result.Failure<Space>(DomainErrors.Space.NotFound(key));

        var result = space.Update(name, description);
        if (result.IsFailure) return Result.Failure<Space>(result.Error);

        await uow.SaveChangesAsync(ct);
        return space;
    }

    /// <inheritdoc />
    public async Task<Result> DeleteAsync(string key, CancellationToken ct)
    {
        var space = await spaceRepo.FirstOrDefaultAsync(new SpaceByKeySpec(key), ct);
        if (space is null) return Result.Failure(DomainErrors.Space.NotFound(key));

        await spaceRepo.DeleteAsync(space, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<PaginatedResult<Space>> GetAllAsync(int offset, int limit, CancellationToken ct)
    {
        (offset, limit) = PaginatedResult<Space>.Clamp(offset, limit);
        var items = await spaceRepo.ListAsync(new SpacesPaginatedSpec(offset, limit), ct);
        var total = await spaceRepo.CountAsync(ct);
        return new PaginatedResult<Space>(items, total, offset, limit);
    }

    /// <inheritdoc />
    public async Task<Result<Space>> GetByKeyAsync(string key, CancellationToken ct)
    {
        var space = await spaceRepo.FirstOrDefaultAsync(new SpaceByKeySpec(key), ct);
        if (space is null) return Result.Failure<Space>(DomainErrors.Space.NotFound(key));
        return space;
    }
}
