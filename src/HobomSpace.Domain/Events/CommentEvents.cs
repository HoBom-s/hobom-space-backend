using HobomSpace.Domain.Common;

namespace HobomSpace.Domain.Events;

public sealed record CommentCreatedEvent(long CommentId, long PageId, string SpaceKey, string ContentPreview, string? ActorId) : DomainEvent;
