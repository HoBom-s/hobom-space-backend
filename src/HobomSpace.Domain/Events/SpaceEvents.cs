using HobomSpace.Domain.Common;

namespace HobomSpace.Domain.Events;

public sealed record SpaceCreatedEvent(long SpaceId, string Key, string Name, string? ActorId) : DomainEvent;
