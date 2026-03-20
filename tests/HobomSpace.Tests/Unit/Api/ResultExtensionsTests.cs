using FluentAssertions;
using HobomSpace.Api.Contracts;
using HobomSpace.Api.Extensions;
using HobomSpace.Domain.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HobomSpace.Tests.Unit.Api;

public class ResultExtensionsTests
{
    [Fact]
    public void ToHttpResult_Success_CallsOnSuccess()
    {
        var result = Result.Success("hello");

        var httpResult = result.ToHttpResult(v => Results.Ok(v));

        httpResult.Should().BeOfType<Ok<string>>();
    }

    [Fact]
    public void ToHttpResult_NotFoundError_Returns404()
    {
        var result = Result.Failure<string>(new Error("Page.NotFound", "Page not found"));

        var httpResult = result.ToHttpResult(v => Results.Ok(v));

        httpResult.Should().BeOfType<NotFound<ApiResponse<object?>>>();
    }

    [Fact]
    public void ToHttpResult_AlreadyExistsError_Returns409()
    {
        var result = Result.Failure<string>(new Error("Space.AlreadyExists", "Space already exists"));

        var httpResult = result.ToHttpResult(v => Results.Ok(v));

        httpResult.Should().BeOfType<Conflict<ApiResponse<object?>>>();
    }

    [Fact]
    public void ToHttpResult_AlreadyAssignedError_Returns409()
    {
        var result = Result.Failure<string>(new Error("Label.AlreadyAssigned", "Already assigned"));

        var httpResult = result.ToHttpResult(v => Results.Ok(v));

        httpResult.Should().BeOfType<Conflict<ApiResponse<object?>>>();
    }

    [Fact]
    public void ToHttpResult_GenericError_Returns400()
    {
        var result = Result.Failure<string>(new Error("Validation.Invalid", "Invalid input"));

        var httpResult = result.ToHttpResult(v => Results.Ok(v));

        httpResult.Should().BeOfType<BadRequest<ApiResponse<object?>>>();
    }

    [Fact]
    public void ToHttpResult_NonGeneric_Success_CallsOnSuccess()
    {
        var result = Result.Success();

        var httpResult = result.ToHttpResult(() => Results.Ok("done"));

        httpResult.Should().BeOfType<Ok<string>>();
    }

    [Fact]
    public void ToHttpResult_NonGeneric_Error_Returns400()
    {
        var result = Result.Failure(new Error("Validation.Invalid", "Bad"));

        var httpResult = result.ToHttpResult(() => Results.Ok());

        httpResult.Should().BeOfType<BadRequest<ApiResponse<object?>>>();
    }
}
