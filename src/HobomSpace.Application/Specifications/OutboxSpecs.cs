using Ardalis.Specification;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Application.Specifications;

public sealed class OutboxByEventTypeAndStatusSpec : Specification<OutboxMessage>
{
    public OutboxByEventTypeAndStatusSpec(string eventType, string status)
        => Query.Where(o => o.EventType == eventType && o.Status == status)
                .OrderBy(o => o.CreatedAt);
}

public sealed class OutboxByEventIdSpec : Specification<OutboxMessage>, ISingleResultSpecification<OutboxMessage>
{
    public OutboxByEventIdSpec(string eventId) => Query.Where(o => o.EventId == eventId);
}

public sealed class OldSentOutboxSpec : Specification<OutboxMessage>
{
    public OldSentOutboxSpec(DateTime cutoff)
        => Query.Where(o => o.Status == "SENT" && o.SentAt < cutoff);
}
