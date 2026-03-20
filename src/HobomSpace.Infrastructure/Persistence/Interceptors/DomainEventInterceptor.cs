using System.Text.Json;
using HobomSpace.Domain.Common;
using HobomSpace.Domain.Entities;
using HobomSpace.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HobomSpace.Infrastructure.Persistence.Interceptors;

public sealed class DomainEventInterceptor : SaveChangesInterceptor
{
    private static readonly AsyncLocal<bool> IsProcessing = new();

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (IsProcessing.Value || eventData.Context is null)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        IsProcessing.Value = true;
        try
        {
            var context = eventData.Context;
            var aggregates = context.ChangeTracker.Entries<IHasDomainEvents>()
                .Where(e => e.Entity.DomainEvents.Count > 0)
                .Select(e => e.Entity)
                .ToList();

            var outboxMessages = new List<OutboxMessage>();
            foreach (var aggregate in aggregates)
            {
                foreach (var domainEvent in aggregate.DomainEvents)
                {
                    var outbox = MapToOutbox(domainEvent);
                    if (outbox is not null)
                        outboxMessages.Add(outbox);
                }
                aggregate.ClearDomainEvents();
            }

            if (outboxMessages.Count > 0)
                await context.Set<OutboxMessage>().AddRangeAsync(outboxMessages, cancellationToken);

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
        finally
        {
            IsProcessing.Value = false;
        }
    }

    private static OutboxMessage? MapToOutbox(DomainEvent domainEvent) => domainEvent switch
    {
        PageCreatedEvent e => OutboxMessage.Create("SPACE_EVENT",
            JsonSerializer.Serialize(new { entityType = "PAGE", action = "CREATED", spaceKey = e.SpaceKey, pageId = e.PageId, title = e.Title, actorId = e.ActorId ?? "" })),

        PageUpdatedEvent e => OutboxMessage.Create("SPACE_EVENT",
            JsonSerializer.Serialize(new { entityType = "PAGE", action = "UPDATED", spaceKey = e.SpaceKey, pageId = e.PageId, title = e.Title, actorId = e.ActorId ?? "" })),

        PageDeletedEvent e => OutboxMessage.Create("SPACE_EVENT",
            JsonSerializer.Serialize(new { entityType = "PAGE", action = "DELETED", spaceKey = e.SpaceKey, pageId = e.PageId, title = e.Title, actorId = e.ActorId ?? "" })),

        PageMovedEvent e => OutboxMessage.Create("SPACE_EVENT",
            JsonSerializer.Serialize(new { entityType = "PAGE", action = "MOVED", spaceKey = e.SpaceKey, pageId = e.PageId, title = e.Title, actorId = e.ActorId ?? "" })),

        CommentCreatedEvent e => OutboxMessage.Create("SPACE_EVENT",
            JsonSerializer.Serialize(new { entityType = "COMMENT", action = "CREATED", spaceKey = e.SpaceKey, pageId = e.PageId, title = e.ContentPreview, actorId = e.ActorId ?? "" })),

        _ => null
    };
}
