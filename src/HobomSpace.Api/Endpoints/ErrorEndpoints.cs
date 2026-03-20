using HobomSpace.Api.Contracts;
using HobomSpace.Api.Extensions;
using HobomSpace.Application.Services;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Api.Endpoints;

/// <summary>클라이언트 에러 캡처 및 조회 엔드포인트.</summary>
public static class ErrorEndpoints
{
    /// <summary>에러 관련 엔드포인트를 매핑한다.</summary>
    public static RouteGroupBuilder MapErrorEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/errors").WithTags("Errors");

        group.MapPost("/", async (CaptureErrorRequest request,
            IErrorService service, HttpContext context, CancellationToken ct) =>
        {
            var nickname = context.Request.Headers["X-User-Nickname"].FirstOrDefault();
            var result = await service.CaptureAsync(
                request.Message, request.StackTrace, request.Screen,
                request.ErrorType, request.UserAgent, nickname, ct);
            return result.ToHttpResult(errorEvent =>
                Results.Created($"/api/v1/errors/{errorEvent.Id}", ApiResponse.Created(ToResponse(errorEvent))));
        }).Produces<ApiResponse<ErrorEventResponse>>(StatusCodes.Status201Created);

        group.MapGet("/", async (IErrorService service, CancellationToken ct,
            string? errorType = null, string? screen = null, int page = 0, int size = 20) =>
        {
            var result = await service.GetAllAsync(page, size, errorType, screen, ct);
            var offset = result.Page * result.Size;
            return Results.Ok(ApiResponse.Ok(new PaginatedResponse<ErrorEventResponse>(
                result.Items.Select(ToResponse).ToList(), result.TotalCount, offset, result.Size)));
        }).Produces<ApiResponse<PaginatedResponse<ErrorEventResponse>>>();

        group.MapGet("/{id:long}", async (long id, IErrorService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result.ToHttpResult(errorEvent => Results.Ok(ApiResponse.Ok(ToResponse(errorEvent))));
        }).Produces<ApiResponse<ErrorEventResponse>>();

        return group;
    }

    private static ErrorEventResponse ToResponse(ErrorEvent e) =>
        new(e.Id, e.Message, e.StackTrace, e.Screen, e.ErrorType, e.UserAgent, e.Nickname, e.CreatedAt);
}
