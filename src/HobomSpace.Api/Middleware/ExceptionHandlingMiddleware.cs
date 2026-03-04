using HobomSpace.Api.Contracts;
using HobomSpace.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HobomSpace.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            ConflictException => (StatusCodes.Status409Conflict, exception.Message),
            ArgumentException => (StatusCodes.Status400BadRequest, exception.Message),
            DbUpdateException dbEx when IsUniqueConstraintViolation(dbEx)
                => (StatusCodes.Status409Conflict, "Resource already exists."),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred."),
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception");
        else
            logger.LogWarning(exception, "{Message}", message);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(ApiResponse.Error(message));
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
        => ex.InnerException is PostgresException { SqlState: "23505" };
}
