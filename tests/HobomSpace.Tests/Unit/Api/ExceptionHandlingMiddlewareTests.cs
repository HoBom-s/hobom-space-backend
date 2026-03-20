using FluentAssertions;
using HobomSpace.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using NSubstitute;

namespace HobomSpace.Tests.Unit.Api;

public class ExceptionHandlingMiddlewareTests
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = Substitute.For<ILogger<ExceptionHandlingMiddleware>>();

    [Fact]
    public async Task ArgumentException_Returns400()
    {
        var middleware = new ExceptionHandlingMiddleware(_ => throw new ArgumentException("bad arg"), _logger);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task DbUpdateException_UniqueViolation_Returns409()
    {
        var pgEx = CreatePostgresException("23505");
        var dbEx = new DbUpdateException("conflict", pgEx);
        var middleware = new ExceptionHandlingMiddleware(_ => throw dbEx, _logger);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task DbUpdateException_NonUnique_Returns500()
    {
        var dbEx = new DbUpdateException("other error", new Exception("inner"));
        var middleware = new ExceptionHandlingMiddleware(_ => throw dbEx, _logger);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task UnhandledException_Returns500()
    {
        var middleware = new ExceptionHandlingMiddleware(_ => throw new InvalidOperationException("boom"), _logger);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(500);
    }

    private static PostgresException CreatePostgresException(string sqlState)
    {
        // PostgresException constructor is internal; use reflection
        var ctor = typeof(PostgresException).GetConstructor(
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
            null,
            [typeof(string), typeof(string), typeof(string), typeof(string)],
            null);

        if (ctor != null)
            return (PostgresException)ctor.Invoke([sqlState, "ERROR", "23505", "duplicate key"]);

        // Fallback: use the public constructor with MessageText
        return new PostgresException(
            messageText: "duplicate key value violates unique constraint",
            severity: "ERROR",
            invariantSeverity: "ERROR",
            sqlState: sqlState);
    }
}
