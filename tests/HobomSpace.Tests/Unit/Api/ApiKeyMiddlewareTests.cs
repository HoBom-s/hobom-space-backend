using FluentAssertions;
using HobomSpace.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace HobomSpace.Tests.Unit.Api;

public class ApiKeyMiddlewareTests
{
    private const string ValidKey = "test-api-key";

    private static IConfiguration CreateConfig(string? apiKey = ValidKey) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(apiKey != null
                ? new[] { new KeyValuePair<string, string?>("Security:ApiKey", apiKey) }
                : [])
            .Build();

    [Fact]
    public async Task ValidKey_CallsNext()
    {
        var nextCalled = false;
        var middleware = new ApiKeyMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, CreateConfig());
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Api-Key"] = ValidKey;

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task MissingHeader_Returns401()
    {
        var middleware = new ApiKeyMiddleware(_ => Task.CompletedTask, CreateConfig());
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task WrongKey_Returns403()
    {
        var middleware = new ApiKeyMiddleware(_ => Task.CompletedTask, CreateConfig());
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Api-Key"] = "wrong-key";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task GrpcContentType_BypassesAuth()
    {
        var nextCalled = false;
        var middleware = new ApiKeyMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, CreateConfig());
        var context = new DefaultHttpContext();
        context.Request.ContentType = "application/grpc";

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task HealthPath_BypassesAuth()
    {
        var nextCalled = false;
        var middleware = new ApiKeyMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, CreateConfig());
        var context = new DefaultHttpContext();
        context.Request.Path = "/health/live";

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }
}
