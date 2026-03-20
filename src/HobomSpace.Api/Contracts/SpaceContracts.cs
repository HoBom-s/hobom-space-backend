namespace HobomSpace.Api.Contracts;

/// <summary>Space 생성 요청 DTO.</summary>
public record CreateSpaceRequest(string Key, string Name, string? Description);

/// <summary>Space 수정 요청 DTO.</summary>
public record UpdateSpaceRequest(string Name, string? Description);

/// <summary>Space 응답 DTO.</summary>
public record SpaceResponse(long Id, string Key, string Name, string? Description, DateTime CreatedAt, DateTime UpdatedAt);
