using HobomSpace.Application.Models;
using HobomSpace.Application.Ports;
using HobomSpace.Application.Specifications;
using HobomSpace.Domain.Common;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Application.Services;

/// <summary>댓글 CRUD 연산을 정의한다. 대댓글(스레드)을 지원한다.</summary>
public interface ICommentService
{
    /// <summary>페이지에 새 댓글을 생성한다. 부모 댓글 지정 시 대댓글이 된다.</summary>
    Task<Result<Comment>> CreateAsync(string spaceKey, long pageId, long? parentCommentId, string content, string? author, string? actorId, CancellationToken ct = default);

    /// <summary>댓글 본문을 수정한다.</summary>
    Task<Result<Comment>> UpdateAsync(long commentId, string content, CancellationToken ct = default);

    /// <summary>댓글을 삭제한다.</summary>
    Task<Result> DeleteAsync(long commentId, CancellationToken ct = default);

    /// <summary>페이지에 달린 댓글 목록을 페이지네이션하여 조회한다.</summary>
    Task<PaginatedResult<Comment>> GetByPageIdAsync(long pageId, int offset, int limit, CancellationToken ct = default);
}

/// <summary>댓글 CRUD 서비스 구현체.</summary>
public sealed class CommentService(IRepository<Page> pageRepo, IRepository<Comment> commentRepo, IUnitOfWork uow) : ICommentService
{
    public async Task<Result<Comment>> CreateAsync(string spaceKey, long pageId, long? parentCommentId, string content, string? author, string? actorId, CancellationToken ct)
    {
        var page = await pageRepo.FirstOrDefaultAsync(new PageByIdSpec(pageId), ct);
        if (page is null) return Result.Failure<Comment>(DomainErrors.Page.NotFound(pageId));

        if (parentCommentId.HasValue)
        {
            var parent = await commentRepo.FirstOrDefaultAsync(new CommentByIdSpec(parentCommentId.Value), ct);
            if (parent is null) return Result.Failure<Comment>(DomainErrors.Comment.ParentNotFound(parentCommentId.Value));
            if (parent.PageId != pageId) return Result.Failure<Comment>(DomainErrors.Comment.ParentOnDifferentPage);
        }

        var commentResult = Comment.Create(page, parentCommentId, content, author, spaceKey, actorId);
        if (commentResult.IsFailure) return Result.Failure<Comment>(commentResult.Error);

        await commentRepo.AddAsync(commentResult.Value, ct);
        await uow.SaveChangesAsync(ct);
        return commentResult;
    }

    public async Task<Result<Comment>> UpdateAsync(long commentId, string content, CancellationToken ct)
    {
        var comment = await commentRepo.FirstOrDefaultAsync(new CommentByIdSpec(commentId), ct);
        if (comment is null) return Result.Failure<Comment>(DomainErrors.Comment.NotFound(commentId));

        var result = comment.Update(content);
        if (result.IsFailure) return Result.Failure<Comment>(result.Error);

        await uow.SaveChangesAsync(ct);
        return comment;
    }

    public async Task<Result> DeleteAsync(long commentId, CancellationToken ct)
    {
        var comment = await commentRepo.FirstOrDefaultAsync(new CommentByIdSpec(commentId), ct);
        if (comment is null) return Result.Failure(DomainErrors.Comment.NotFound(commentId));

        await commentRepo.DeleteAsync(comment, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<PaginatedResult<Comment>> GetByPageIdAsync(long pageId, int offset, int limit, CancellationToken ct)
    {
        (offset, limit) = PaginatedResult<Comment>.Clamp(offset, limit);
        var items = await commentRepo.ListAsync(new CommentsByPageIdSpec(pageId, offset, limit), ct);
        var total = await commentRepo.CountAsync(new CommentCountByPageIdSpec(pageId), ct);
        return new PaginatedResult<Comment>(items, total, offset, limit);
    }
}
