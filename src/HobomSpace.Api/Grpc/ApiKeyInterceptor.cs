using System.Security.Cryptography;
using System.Text;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace HobomSpace.Api.Grpc;

public sealed class ApiKeyInterceptor(IConfiguration configuration) : Interceptor
{
    private const string ApiKeyHeader = "x-api-key";

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var metadata = context.RequestHeaders;
        var providedKey = metadata.GetValue(ApiKeyHeader);

        var expectedKey = configuration["Security:ApiKey"];

        if (string.IsNullOrEmpty(providedKey)
            || string.IsNullOrEmpty(expectedKey)
            || !IsKeyValid(providedKey, expectedKey))
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid or missing API key"));
        }

        return await continuation(request, context);
    }

    private static bool IsKeyValid(string provided, string expected)
    {
        var providedBytes = Encoding.UTF8.GetBytes(provided);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        return CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes);
    }
}
