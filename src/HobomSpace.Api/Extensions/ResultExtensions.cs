using HobomSpace.Api.Contracts;
using HobomSpace.Domain.Common;

namespace HobomSpace.Api.Extensions;

public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult> onSuccess)
    {
        if (result.IsSuccess) return onSuccess(result.Value);
        return MapError(result.Error);
    }

    public static IResult ToHttpResult(this Result result, Func<IResult> onSuccess)
    {
        if (result.IsSuccess) return onSuccess();
        return MapError(result.Error);
    }

    public static async Task<IResult> ToHttpResultAsync<T>(this Result<T> result, Func<T, Task<IResult>> onSuccess)
    {
        if (result.IsSuccess) return await onSuccess(result.Value);
        return MapError(result.Error);
    }

    private static IResult MapError(Error error) => error.Code switch
    {
        var c when c.Contains("NotFound") => Results.NotFound(ApiResponse.Error(error.Description)),
        var c when c.Contains("AlreadyExists") || c.Contains("AlreadyAssigned") => Results.Conflict(ApiResponse.Error(error.Description)),
        _ => Results.BadRequest(ApiResponse.Error(error.Description)),
    };
}
