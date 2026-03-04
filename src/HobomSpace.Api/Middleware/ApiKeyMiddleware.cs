using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace HobomSpace.Api.Middleware;

public sealed class ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private const string ApiKeyHeader = "X-Api-Key";

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/health/", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/openapi/", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/scalar/", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var providedKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = "API key is required.",
            });
            return;
        }

        var expectedKey = configuration["Security:ApiKey"];
        if (string.IsNullOrEmpty(expectedKey) || !IsKeyValid(providedKey!, expectedKey))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Detail = "Invalid API key.",
            });
            return;
        }

        await next(context);
    }

    private static bool IsKeyValid(string provided, string expected)
    {
        var providedBytes = Encoding.UTF8.GetBytes(provided);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        return CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes);
    }
}
