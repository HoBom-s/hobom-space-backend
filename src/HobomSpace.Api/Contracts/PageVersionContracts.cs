namespace HobomSpace.Api.Contracts;

/// <summary>페이지 버전 응답 DTO.</summary>
public record PageVersionResponse(long Id, long PageId, int Version, string Title, string Content, string? EditedBy, DateTime CreatedAt);

/// <summary>버전 diff 결과 항목 DTO.</summary>
public record DiffEntryResponse(int LineNumber, string Type, string Content);
