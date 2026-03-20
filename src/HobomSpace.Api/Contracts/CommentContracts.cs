namespace HobomSpace.Api.Contracts;

/// <summary>댓글 생성 요청 DTO.</summary>
public record CreateCommentRequest(string Content, long? ParentCommentId, string? Author);

/// <summary>댓글 수정 요청 DTO.</summary>
public record UpdateCommentRequest(string Content);

/// <summary>댓글 응답 DTO.</summary>
public record CommentResponse(long Id, long PageId, long? ParentCommentId, string Content, string? Author, DateTime CreatedAt, DateTime UpdatedAt);
