using HobomSpace.Application.Models;
using HobomSpace.Application.Ports;
using HobomSpace.Domain.Entities;
using HobomSpace.Domain.Exceptions;

namespace HobomSpace.Application.Services;

public interface IErrorEventService
{
    Task<ErrorEvent> CaptureAsync(string message, string? stackTrace, string screen, string errorType, string? userAgent, string? nickname, CancellationToken ct = default);
    Task<PaginatedResult<ErrorEvent>> GetAllAsync(int offset, int limit, string? errorType = null, string? screen = null, CancellationToken ct = default);
    Task<ErrorEvent> GetByIdAsync(long id, CancellationToken ct = default);
}

public sealed class ErrorEventService(
    IErrorEventRepository errorEventRepo,
    IUnitOfWork unitOfWork) : IErrorEventService
{
    public async Task<ErrorEvent> CaptureAsync(string message, string? stackTrace, string screen, string errorType, string? userAgent, string? nickname, CancellationToken ct = default)
    {
        var errorEvent = ErrorEvent.Create(message, stackTrace, screen, errorType, userAgent, nickname);
        await errorEventRepo.AddAsync(errorEvent, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return errorEvent;
    }

    public async Task<PaginatedResult<ErrorEvent>> GetAllAsync(int offset, int limit, string? errorType = null, string? screen = null, CancellationToken ct = default)
    {
        (offset, limit) = PaginatedResult<ErrorEvent>.Clamp(offset, limit);
        var items = await errorEventRepo.GetAllAsync(offset, limit, errorType, screen, ct);
        var total = await errorEventRepo.CountAsync(errorType, screen, ct);
        return new PaginatedResult<ErrorEvent>(items, total, offset, limit);
    }

    public async Task<ErrorEvent> GetByIdAsync(long id, CancellationToken ct = default)
        => await errorEventRepo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"ErrorEvent with id {id} not found");
}
