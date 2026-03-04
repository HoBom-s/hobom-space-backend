namespace HobomSpace.Api.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string Header = "x-hobom-trace-id";

    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = context.Request.Headers[Header].FirstOrDefault()
                      ?? Guid.NewGuid().ToString("N");

        context.Items["TraceId"] = traceId;
        context.Response.Headers[Header] = traceId;

        using (Serilog.Context.LogContext.PushProperty("TraceId", traceId))
        {
            await next(context);
        }
    }
}
