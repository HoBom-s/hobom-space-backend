using HobomSpace.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
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
        var (statusCode, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
            DbUpdateException dbEx when IsUniqueConstraintViolation(dbEx)
                => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error"),
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception");
        else
            logger.LogWarning(exception, "{Title}: {Detail}", title, exception.Message);

        var correlationId = context.Items["TraceId"]?.ToString();

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred."
                : exception.Message,
            Extensions = { ["traceId"] = correlationId },
        });
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
        => ex.InnerException is PostgresException { SqlState: "23505" };
}
