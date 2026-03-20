using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using HobomSpace.Application.Ports;
using Outbox.Space;

namespace HobomSpace.Api.Grpc;

/// <summary>Outbox 메시지 상태를 gRPC로 변경하는 서비스 (SENT/FAILED).</summary>
public sealed class SpaceOutboxPatchService(IOutboxRepository outboxRepo, IUnitOfWork uow)
    : PatchHoBomSpaceOutboxController.PatchHoBomSpaceOutboxControllerBase
{
    public override async Task<Empty> PatchOutboxMarkAsSentUseCase(MarkRequest request, ServerCallContext context)
    {
        var outbox = await outboxRepo.FindByEventIdAsync(request.EventId, context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Outbox event '{request.EventId}' not found"));

        outbox.MarkAsSent();
        await uow.SaveChangesAsync(context.CancellationToken);

        return new Empty();
    }

    public override async Task<Empty> PatchOutboxMarkAsFailedUseCase(MarkFailedRequest request, ServerCallContext context)
    {
        var outbox = await outboxRepo.FindByEventIdAsync(request.EventId, context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Outbox event '{request.EventId}' not found"));

        outbox.MarkAsFailed(request.ErrorMessage);
        await uow.SaveChangesAsync(context.CancellationToken);

        return new Empty();
    }
}
