using HobomAdmin.Application.Ports.In;

public static class DlqEndpoints
{
    public static void MapDlqEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/dlq").WithTags("DLQ");

        group.MapGet("/", async (string? prefix, IGetDlqKeysUseCase useCase, CancellationToken ct) =>
        {
            var keys = await useCase.ExecuteAsync(prefix ?? "dlq:", ct);
            return Results.Ok(keys);
        });
    }
}
