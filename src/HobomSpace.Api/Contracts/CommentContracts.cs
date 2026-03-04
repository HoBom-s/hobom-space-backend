namespace HobomSpace.Api.Contracts;

public record CreateCommentRequest(string Content, long? ParentCommentId, string? Author);
public record UpdateCommentRequest(string Content);
public record CommentResponse(long Id, long PageId, long? ParentCommentId, string Content, string? Author, DateTime CreatedAt, DateTime UpdatedAt);
