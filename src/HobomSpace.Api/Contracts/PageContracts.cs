namespace HobomSpace.Api.Contracts;

/// <summary>페이지 생성 요청 DTO.</summary>
public record CreatePageRequest(string Title, string Content, long? ParentPageId, int Position = 0);

/// <summary>페이지 수정 요청 DTO.</summary>
public record UpdatePageRequest(string Title, string Content, int? Position);

/// <summary>페이지 응답 DTO.</summary>
public record PageResponse(long Id, long SpaceId, long? ParentPageId, string Title, string Content, int Position, DateTime CreatedAt, DateTime UpdatedAt);

/// <summary>페이지 이동 요청 DTO.</summary>
public record MovePageRequest(string TargetSpaceKey, long? ParentPageId);

/// <summary>페이지 복사 요청 DTO.</summary>
public record CopyPageRequest(string TargetSpaceKey, long? ParentPageId);

/// <summary>페이지 트리 노드 DTO (재귀 구조).</summary>
public record PageTreeNode(long Id, string Title, int Position, List<PageTreeNode> Children);
