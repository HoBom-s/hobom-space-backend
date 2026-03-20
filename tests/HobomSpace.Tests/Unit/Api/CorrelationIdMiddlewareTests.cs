using FluentAssertions;
using HobomSpace.Api.Middleware;
using Microsoft.AspNetCore.Http;

namespace HobomSpace.Tests.Unit.Api;

public class CorrelationIdMiddlewareTests
{
    private const string Header = "x-hobom-trace-id";

    [Fact]
    public async Task NoHeader_GeneratesGuid()
    {
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        var traceId = context.Items["TraceId"] as string;
        traceId.Should().NotBeNullOrEmpty();
        traceId.Should().HaveLength(32); // Guid.ToString("N")
        context.Response.Headers[Header].ToString().Should().Be(traceId);
    }

    [Fact]
    public async Task ExistingHeader_Propagates()
    {
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);
        var context = new DefaultHttpContext();
        context.Request.Headers[Header] = "custom-trace-id";

        await middleware.InvokeAsync(context);

        var traceId = context.Items["TraceId"] as string;
        traceId.Should().Be("custom-trace-id");
        context.Response.Headers[Header].ToString().Should().Be("custom-trace-id");
    }

    [Fact]
    public async Task ResponseHeader_ContainsTraceId()
    {
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.Headers.Should().ContainKey(Header);
    }
}
