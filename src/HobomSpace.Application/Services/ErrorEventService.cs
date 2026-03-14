using HobomSpace.Application.Models;
using HobomSpace.Application.Ports;
using HobomSpace.Domain.Entities;
using HobomSpace.Domain.Exceptions;

namespace HobomSpace.Application.Services;

public interface IErrorEventService
{
    Task<ErrorEvent> CaptureAsync(string message, string? stackTrace, string screen, string errorType, string? userAgent, string? nickname, CancellationToken ct = default);
    Task<PagedResult<ErrorEvent>> GetAllAsync(int page, int size, string? errorType = null, string? screen = null, CancellationToken ct = default);
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

    public async Task<PagedResult<ErrorEvent>> GetAllAsync(int page, int size, string? errorType = null, string? screen = null, CancellationToken ct = default)
    {
        (page, size) = PagedResult<ErrorEvent>.Clamp(page, size);
        var offset = (page - 1) * size;
        var items = await errorEventRepo.GetAllAsync(offset, size, errorType, screen, ct);
        var total = await errorEventRepo.CountAsync(errorType, screen, ct);
        return new PagedResult<ErrorEvent>(items, total, page, size);
    }

    public async Task<ErrorEvent> GetByIdAsync(long id, CancellationToken ct = default)
        => await errorEventRepo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"ErrorEvent with id {id} not found");
}
