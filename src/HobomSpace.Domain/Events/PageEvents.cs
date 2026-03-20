using HobomSpace.Domain.Common;

namespace HobomSpace.Domain.Events;

public sealed record PageCreatedEvent(long PageId, long SpaceId, string SpaceKey, string Title, string? ActorId) : DomainEvent;
public sealed record PageUpdatedEvent(long PageId, long SpaceId, string SpaceKey, string Title, string? ActorId) : DomainEvent;
public sealed record PageDeletedEvent(long PageId, long SpaceId, string SpaceKey, string Title, string? ActorId) : DomainEvent;
public sealed record PageMovedEvent(long PageId, long SpaceId, string SpaceKey, string Title, string? ActorId) : DomainEvent;
