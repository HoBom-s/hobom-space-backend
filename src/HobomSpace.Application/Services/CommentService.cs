using System.Text.Json;
using HobomSpace.Application.Models;
using HobomSpace.Application.Ports;
using HobomSpace.Domain.Entities;
using HobomSpace.Domain.Exceptions;

namespace HobomSpace.Application.Services;

public interface ICommentService
{
    Task<Comment> CreateAsync(string spaceKey, long pageId, long? parentCommentId, string content, string? author, string? actorId = null, CancellationToken ct = default);
    Task<PaginatedResult<Comment>> GetByPageIdAsync(long pageId, int offset, int limit, CancellationToken ct = default);
    Task<Comment> UpdateAsync(long commentId, string content, CancellationToken ct = default);
    Task DeleteAsync(long commentId, CancellationToken ct = default);
}

public sealed class CommentService(
    ICommentRepository commentRepo,
    IPageRepository pageRepo,
    IOutboxRepository outboxRepo,
    IUnitOfWork unitOfWork) : ICommentService
{
    public async Task<Comment> CreateAsync(string spaceKey, long pageId, long? parentCommentId, string content, string? author, string? actorId = null, CancellationToken ct = default)
    {
        _ = await pageRepo.GetByIdAsync(pageId, ct)
            ?? throw new NotFoundException($"Page {pageId} not found.");

        if (parentCommentId.HasValue)
        {
            var parent = await commentRepo.GetByIdAsync(parentCommentId.Value, ct)
                ?? throw new NotFoundException($"Parent comment {parentCommentId} not found.");
            if (parent.PageId != pageId)
                throw new ArgumentException("Parent comment does not belong to the specified page.");
        }

        var comment = Comment.Create(pageId, parentCommentId, content, author);
        await commentRepo.AddAsync(comment, ct);
        await outboxRepo.AddAsync(OutboxMessage.Create("SPACE_EVENT",
            JsonSerializer.Serialize(new { entityType = "COMMENT", action = "CREATED", spaceKey, pageId, title = content.Length > 100 ? content[..100] : content, actorId = actorId ?? "" })), ct);
        await unitOfWork.SaveChangesAsync(ct);
        return comment;
    }

    public async Task<PaginatedResult<Comment>> GetByPageIdAsync(long pageId, int offset, int limit, CancellationToken ct = default)
    {
        (offset, limit) = PaginatedResult<Comment>.Clamp(offset, limit);
        var items = await commentRepo.GetByPageIdAsync(pageId, offset, limit, ct);
        var total = await commentRepo.CountByPageIdAsync(pageId, ct);
        return new PaginatedResult<Comment>(items, total, offset, limit);
    }

    public async Task<Comment> UpdateAsync(long commentId, string content, CancellationToken ct = default)
    {
        var comment = await GetCommentAsyncById(commentId, ct);
        comment.Update(content);
        await unitOfWork.SaveChangesAsync(ct);
        return comment;
    }

    public async Task DeleteAsync(long commentId, CancellationToken ct = default)
    {
        var comment = await GetCommentAsyncById(commentId, ct);
        commentRepo.Remove(comment);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task<Comment> GetCommentAsyncById(long commentId, CancellationToken ct = default)
        => await commentRepo.GetByIdAsync(commentId, ct)
            ?? throw new NotFoundException($"Comment with id {commentId} not found");
}
