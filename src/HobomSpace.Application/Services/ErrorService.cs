using HobomSpace.Application.Models;
using HobomSpace.Application.Ports;
using HobomSpace.Application.Specifications;
using HobomSpace.Domain.Common;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Application.Services;

/// <summary>클라이언트 에러 캡처 및 조회 연산을 정의한다.</summary>
public interface IErrorService
{
    /// <summary>클라이언트 에러를 캡처하여 저장한다.</summary>
    Task<Result<ErrorEvent>> CaptureAsync(string message, string? stackTrace, string screen, string errorType, string? userAgent, string? nickname, CancellationToken ct = default);

    /// <summary>에러 이벤트 목록을 필터링·페이지네이션하여 조회한다.</summary>
    Task<PagedResult<ErrorEvent>> GetAllAsync(int page, int size, string? errorType, string? screen, CancellationToken ct = default);

    /// <summary>ID로 에러 이벤트를 조회한다.</summary>
    Task<Result<ErrorEvent>> GetByIdAsync(long id, CancellationToken ct = default);
}

/// <summary>에러 캡처 서비스 구현체.</summary>
public sealed class ErrorService(IRepository<ErrorEvent> errorRepo, IUnitOfWork uow) : IErrorService
{
    public async Task<Result<ErrorEvent>> CaptureAsync(string message, string? stackTrace, string screen, string errorType, string? userAgent, string? nickname, CancellationToken ct)
    {
        var errorEvent = ErrorEvent.Create(message, stackTrace, screen, errorType, userAgent, nickname);
        await errorRepo.AddAsync(errorEvent, ct);
        await uow.SaveChangesAsync(ct);
        return errorEvent;
    }

    public async Task<PagedResult<ErrorEvent>> GetAllAsync(int page, int size, string? errorType, string? screen, CancellationToken ct)
    {
        page = Math.Max(0, page);
        size = Math.Clamp(size, 1, 100);
        var items = await errorRepo.ListAsync(new ErrorEventsPaginatedSpec(page, size, errorType, screen), ct);
        var total = await errorRepo.CountAsync(new ErrorEventsCountSpec(errorType, screen), ct);
        return new PagedResult<ErrorEvent>(items, total, page, size);
    }

    public async Task<Result<ErrorEvent>> GetByIdAsync(long id, CancellationToken ct)
    {
        var errorEvent = await errorRepo.FirstOrDefaultAsync(new ErrorEventByIdSpec(id), ct);
        if (errorEvent is null) return Result.Failure<ErrorEvent>(DomainErrors.ErrorEvent.NotFound(id));
        return errorEvent;
    }
}
