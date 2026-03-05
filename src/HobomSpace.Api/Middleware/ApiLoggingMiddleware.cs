using System.Text.Json;
using HobomSpace.Application.Ports;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Api.Middleware;

public sealed class ApiLoggingMiddleware(RequestDelegate next)
{
    private static readonly HashSet<string> ExcludedPrefixes =
    [
        "/health",
        "/openapi",
        "/scalar",
    ];

    public async Task InvokeAsync(HttpContext context, IServiceScopeFactory scopeFactory)
    {
        await next(context);

        if (!ShouldLog(context)) return;

        using var scope = scopeFactory.CreateScope();
        var outboxRepo = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var traceId = context.Items["TraceId"]?.ToString() ?? context.TraceIdentifier;
        var payload = new
        {
            serviceType = "HOBOM_SPACE",
            level = context.Response.StatusCode >= 500 ? "ERROR" : "INFO",
            traceId,
            message = $"{context.Request.Method} {context.Request.Path} - {context.Response.StatusCode}",
            method = context.Request.Method,
            path = context.Request.Path.ToString(),
            statusCode = context.Response.StatusCode,
            host = context.Request.Host.ToString(),
            userId = "",
            payload = new { query = new Dictionary<string, string>(), body = new Dictionary<string, string>(), headers = new Dictionary<string, string>(), error = "" },
        };

        await outboxRepo.AddAsync(OutboxMessage.Create("SPACE_LOG",
            JsonSerializer.Serialize(payload)));
        await uow.SaveChangesAsync();
    }

    private static bool ShouldLog(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (context.Request.Protocol == "HTTP/2" && string.IsNullOrEmpty(path))
            return false;

        foreach (var prefix in ExcludedPrefixes)
        {
            if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }
}
