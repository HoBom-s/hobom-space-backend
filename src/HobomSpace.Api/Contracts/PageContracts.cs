namespace HobomSpace.Api.Contracts;

public record CreatePageRequest(string Title, string Content, long? ParentPageId, int Position = 0);
public record UpdatePageRequest(string Title, string Content, int? Position);
public record PageResponse(long Id, long SpaceId, long? ParentPageId, string Title, string Content, int Position, DateTime CreatedAt, DateTime UpdatedAt);
public record PageTreeNode(long Id, string Title, int Position, List<PageTreeNode> Children);
