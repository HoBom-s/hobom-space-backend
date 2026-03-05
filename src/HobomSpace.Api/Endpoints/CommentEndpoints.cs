using HobomSpace.Api.Contracts;
using HobomSpace.Application.Services;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Api.Endpoints;

public static class CommentEndpoints
{
    public static RouteGroupBuilder MapCommentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/spaces/{spaceKey}/pages/{pageId:long}/comments").WithTags("Comments");

        group.MapPost("/", async (string spaceKey, long pageId, CreateCommentRequest request,
            IPageService pageService, ICommentService service, HttpContext context, CancellationToken ct) =>
        {
            await pageService.GetByIdAsync(spaceKey, pageId, ct);
            var actorId = context.Request.Headers["X-User-Id"].FirstOrDefault();
            var comment = await service.CreateAsync(spaceKey, pageId, request.ParentCommentId, request.Content, request.Author, actorId, ct);
            return Results.Created($"/api/v1/spaces/{spaceKey}/pages/{pageId}/comments/{comment.Id}", ApiResponse.Created(ToResponse(comment)));
        }).Produces<ApiResponse<CommentResponse>>(StatusCodes.Status201Created);

        group.MapGet("/", async (string spaceKey, long pageId,
            IPageService pageService, ICommentService service, CancellationToken ct, int offset = 0, int limit = 20) =>
        {
            await pageService.GetByIdAsync(spaceKey, pageId, ct);
            var result = await service.GetByPageIdAsync(pageId, offset, limit, ct);
            return Results.Ok(ApiResponse.Ok(new PaginatedResponse<CommentResponse>(
                result.Items.Select(ToResponse).ToList(), result.TotalCount, result.Offset, result.Limit)));
        }).Produces<ApiResponse<PaginatedResponse<CommentResponse>>>();

        group.MapPut("/{commentId:long}", async (string spaceKey, long pageId, long commentId, UpdateCommentRequest request,
            IPageService pageService, ICommentService service, CancellationToken ct) =>
        {
            await pageService.GetByIdAsync(spaceKey, pageId, ct);
            return Results.Ok(ApiResponse.Ok(ToResponse(await service.UpdateAsync(commentId, request.Content, ct))));
        }).Produces<ApiResponse<CommentResponse>>();

        group.MapDelete("/{commentId:long}", async (string spaceKey, long pageId, long commentId,
            IPageService pageService, ICommentService service, CancellationToken ct) =>
        {
            await pageService.GetByIdAsync(spaceKey, pageId, ct);
            await service.DeleteAsync(commentId, ct);
            return Results.Ok(ApiResponse.Ok<object?>(null, "Deleted"));
        }).Produces<ApiResponse<object>>();

        return group;
    }

    private static CommentResponse ToResponse(Comment c) =>
        new(c.Id, c.PageId, c.ParentCommentId, c.Content, c.Author, c.CreatedAt, c.UpdatedAt);
}
