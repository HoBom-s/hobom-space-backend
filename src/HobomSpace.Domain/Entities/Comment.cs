using HobomSpace.Domain.Common;
using HobomSpace.Domain.Events;

namespace HobomSpace.Domain.Entities;

/// <summary>
/// 페이지에 달린 댓글. <see cref="ParentCommentId"/>를 통해 대댓글(스레드)을 지원한다.
/// </summary>
public sealed class Comment : AggregateRoot
{
    public long Id { get; private set; }
    public long PageId { get; private set; }

    /// <summary>부모 댓글 ID. <c>null</c>이면 최상위 댓글.</summary>
    public long? ParentCommentId { get; private set; }

    public string Content { get; private set; } = string.Empty;
    public string? Author { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Comment() { }

    /// <summary>새 댓글을 생성한다.</summary>
    public static Result<Comment> Create(Page page, long? parentCommentId, string content, string? author, string spaceKey, string? actorId)
    {
        if (string.IsNullOrWhiteSpace(content))
            return Result.Failure<Comment>(new Error("Comment.ContentEmpty", "Content cannot be null or whitespace."));

        var now = DateTime.UtcNow;
        var comment = new Comment
        {
            PageId = page.Id,
            ParentCommentId = parentCommentId,
            Content = content,
            Author = author,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var preview = content.Length > 100 ? content[..100] : content;
        comment.RaiseDomainEvent(new CommentCreatedEvent(comment.Id, page.Id, spaceKey, preview, actorId));
        return comment;
    }

    /// <summary>댓글 본문을 수정한다.</summary>
    public Result Update(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return Result.Failure(new Error("Comment.ContentEmpty", "Content cannot be null or whitespace."));

        Content = content;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
