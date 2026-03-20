using Google.Protobuf;
using Grpc.Core;
using HobomSpace.Application.Ports;
using Outbox.Space;

namespace HobomSpace.Api.Grpc;

/// <summary>SPACE_EVENT Outbox 메시지를 gRPC로 조회하는 서비스.</summary>
public sealed class SpaceOutboxFindService(IOutboxRepository outboxRepo)
    : FindHoBomSpaceOutboxController.FindHoBomSpaceOutboxControllerBase
{
    public override async Task<Response> FindOutboxByEventTypeAndStatusUseCase(Request request, ServerCallContext context)
    {
        var items = await outboxRepo.FindByEventTypeAndStatusAsync(
            request.EventType, request.Status, context.CancellationToken);

        var response = new Response();
        foreach (var item in items)
        {
            var result = new QueryResult
            {
                Id = item.Id.ToString(),
                EventId = item.EventId,
                EventType = item.EventType,
                Status = item.Status,
                RetryCount = item.RetryCount,
                SentAt = item.SentAt?.ToString("o") ?? string.Empty,
                FailedAt = item.FailedAt?.ToString("o") ?? string.Empty,
                LastError = item.LastError ?? string.Empty,
                Version = item.Version,
                CreatedAt = item.CreatedAt.ToString("o"),
                UpdatedAt = item.UpdatedAt.ToString("o"),
                Payload = JsonParser.Default.Parse<SpaceEventPayload>(item.Payload),
            };
            response.Items.Add(result);
        }

        return response;
    }
}
