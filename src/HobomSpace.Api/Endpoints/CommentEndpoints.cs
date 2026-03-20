using HobomSpace.Api.Contracts;
using HobomSpace.Api.Extensions;
using HobomSpace.Application.Services;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Api.Endpoints;

/// <summary>댓글 CRUD 엔드포인트.</summary>
public static class CommentEndpoints
{
    /// <summary>댓글 관련 엔드포인트를 매핑한다.</summary>
    public static RouteGroupBuilder MapCommentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/spaces/{spaceKey}/pages/{pageId:long}/comments").WithTags("Comments");

        group.MapPost("/", async (string spaceKey, long pageId, CreateCommentRequest request,
            ICommentService commentService, HttpContext context, CancellationToken ct) =>
        {
            var actorId = context.Request.Headers["X-User-Nickname"].FirstOrDefault();
            var result = await commentService.CreateAsync(spaceKey, pageId, request.ParentCommentId, request.Content, request.Author, actorId, ct);
            return result.ToHttpResult(comment =>
                Results.Created($"/api/v1/spaces/{spaceKey}/pages/{pageId}/comments/{comment.Id}", ApiResponse.Created(ToResponse(comment))));
        }).Produces<ApiResponse<CommentResponse>>(StatusCodes.Status201Created);

        group.MapGet("/", async (string spaceKey, long pageId,
            IPageService pageService, ICommentService commentService, CancellationToken ct, int offset = 0, int limit = 20) =>
        {
            var pageCheck = await pageService.GetByIdAsync(spaceKey, pageId, ct);
            if (pageCheck.IsFailure) return pageCheck.ToHttpResult(_ => Results.Ok());

            var result = await commentService.GetByPageIdAsync(pageId, offset, limit, ct);
            return Results.Ok(ApiResponse.Ok(new PaginatedResponse<CommentResponse>(
                result.Items.Select(ToResponse).ToList(), result.TotalCount, result.Offset, result.Limit)));
        }).Produces<ApiResponse<PaginatedResponse<CommentResponse>>>();

        group.MapPut("/{commentId:long}", async (string spaceKey, long pageId, long commentId, UpdateCommentRequest request,
            ICommentService commentService, CancellationToken ct) =>
        {
            var result = await commentService.UpdateAsync(commentId, request.Content, ct);
            return result.ToHttpResult(comment => Results.Ok(ApiResponse.Ok(ToResponse(comment))));
        }).Produces<ApiResponse<CommentResponse>>();

        group.MapDelete("/{commentId:long}", async (string spaceKey, long pageId, long commentId,
            ICommentService commentService, CancellationToken ct) =>
        {
            var result = await commentService.DeleteAsync(commentId, ct);
            return result.ToHttpResult(() => Results.Ok(ApiResponse.Ok<object?>(null, "Deleted")));
        }).Produces<ApiResponse<object>>();

        return group;
    }

    private static CommentResponse ToResponse(Comment c) =>
        new(c.Id, c.PageId, c.ParentCommentId, c.Content, c.Author, c.CreatedAt, c.UpdatedAt);
}
