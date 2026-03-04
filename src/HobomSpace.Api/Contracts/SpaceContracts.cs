namespace HobomSpace.Api.Contracts;

public record CreateSpaceRequest(string Key, string Name, string? Description);
public record UpdateSpaceRequest(string Name, string? Description);
public record SpaceResponse(long Id, string Key, string Name, string? Description, DateTime CreatedAt, DateTime UpdatedAt);
