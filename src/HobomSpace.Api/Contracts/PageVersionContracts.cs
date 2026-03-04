namespace HobomSpace.Api.Contracts;

public record PageVersionResponse(long Id, long PageId, int Version, string Title, string Content, string? EditedBy, DateTime CreatedAt);
