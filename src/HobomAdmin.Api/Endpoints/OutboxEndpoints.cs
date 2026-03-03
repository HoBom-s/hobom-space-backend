using HobomAdmin.Application.Ports.In;

public static class OutboxEndpoints
{
    public static void MapOutboxEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/outbox").WithTags("Outbox");

        group.MapGet("/summary", async (IGetOutboxSummaryUseCase useCase, CancellationToken ct) =>
        {
            var summary = await useCase.ExecuteAsync(ct);
            return Results.Ok(summary);
        });
    }
}
