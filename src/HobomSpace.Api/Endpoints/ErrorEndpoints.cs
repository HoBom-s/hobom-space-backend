using HobomSpace.Api.Contracts;
using HobomSpace.Application.Services;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Api.Endpoints;

public static class ErrorEndpoints
{
    public static RouteGroupBuilder MapErrorEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/errors").WithTags("Errors");

        group.MapPost("/", async (CaptureErrorRequest request,
            IErrorEventService service, HttpContext context, CancellationToken ct) =>
        {
            var nickname = context.Request.Headers["X-User-Nickname"].FirstOrDefault();
            var errorEvent = await service.CaptureAsync(
                request.Message, request.StackTrace, request.Screen,
                request.ErrorType, request.UserAgent, nickname, ct);
            return Results.Created($"/api/v1/errors/{errorEvent.Id}", ApiResponse.Created(ToResponse(errorEvent)));
        }).Produces<ApiResponse<ErrorEventResponse>>(StatusCodes.Status201Created);

        group.MapGet("/", async (IErrorEventService service, CancellationToken ct,
            string? errorType = null, string? screen = null, int page = 0, int size = 20) =>
        {
            var result = await service.GetAllAsync(page, size, errorType, screen, ct);
            var offset = result.Page * result.Size;
            return Results.Ok(ApiResponse.Ok(new PaginatedResponse<ErrorEventResponse>(
                result.Items.Select(ToResponse).ToList(), result.TotalCount, offset, result.Size)));
        }).Produces<ApiResponse<PaginatedResponse<ErrorEventResponse>>>();

        group.MapGet("/{id:long}", async (long id, IErrorEventService service, CancellationToken ct) =>
        {
            var errorEvent = await service.GetByIdAsync(id, ct);
            return Results.Ok(ApiResponse.Ok(ToResponse(errorEvent)));
        }).Produces<ApiResponse<ErrorEventResponse>>();

        return group;
    }

    private static ErrorEventResponse ToResponse(ErrorEvent e) =>
        new(e.Id, e.Message, e.StackTrace, e.Screen, e.ErrorType, e.UserAgent, e.Nickname, e.CreatedAt);
}
