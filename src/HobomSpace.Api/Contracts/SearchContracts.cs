namespace HobomSpace.Api.Contracts;

/// <summary>검색 결과 항목 DTO.</summary>
public record SearchResult(long Id, long SpaceId, string Title, DateTime UpdatedAt);
