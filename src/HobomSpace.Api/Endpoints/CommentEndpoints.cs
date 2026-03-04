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
            IPageService pageService, ICommentService service, CancellationToken ct) =>
        {
            await pageService.GetByIdAsync(spaceKey, pageId, ct);
            var comment = await service.CreateAsync(pageId, request.ParentCommentId, request.Content, request.Author, ct);
            return Results.Created($"/api/v1/spaces/{spaceKey}/pages/{pageId}/comments/{comment.Id}", ToResponse(comment));
        }).Produces<CommentResponse>(StatusCodes.Status201Created);

        group.MapGet("/", async (string spaceKey, long pageId,
            IPageService pageService, ICommentService service, CancellationToken ct, int offset = 0, int limit = 20) =>
        {
            await pageService.GetByIdAsync(spaceKey, pageId, ct);
            var result = await service.GetByPageIdAsync(pageId, offset, limit, ct);
            return Results.Ok(new PaginatedResponse<CommentResponse>(
                result.Items.Select(ToResponse).ToList(), result.TotalCount, result.Offset, result.Limit));
        }).Produces<PaginatedResponse<CommentResponse>>();

        group.MapPut("/{commentId:long}", async (string spaceKey, long pageId, long commentId, UpdateCommentRequest request,
            IPageService pageService, ICommentService service, CancellationToken ct) =>
        {
            await pageService.GetByIdAsync(spaceKey, pageId, ct);
            return Results.Ok(ToResponse(await service.UpdateAsync(commentId, request.Content, ct)));
        }).Produces<CommentResponse>();

        group.MapDelete("/{commentId:long}", async (string spaceKey, long pageId, long commentId,
            IPageService pageService, ICommentService service, CancellationToken ct) =>
        {
            await pageService.GetByIdAsync(spaceKey, pageId, ct);
            await service.DeleteAsync(commentId, ct);
            return Results.NoContent();
        }).Produces(StatusCodes.Status204NoContent);

        return group;
    }

    private static CommentResponse ToResponse(Comment c) =>
        new(c.Id, c.PageId, c.ParentCommentId, c.Content, c.Author, c.CreatedAt, c.UpdatedAt);
}
