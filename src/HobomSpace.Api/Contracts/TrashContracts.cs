namespace HobomSpace.Api.Contracts;

/// <summary>휴지통 페이지 응답 DTO.</summary>
public record TrashPageResponse(long Id, long SpaceId, long? ParentPageId, string Title, int Position, DateTime CreatedAt, DateTime? DeletedAt, string? DeletedBy);
