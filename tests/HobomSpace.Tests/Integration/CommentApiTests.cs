using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HobomSpace.Api.Contracts;

namespace HobomSpace.Tests.Integration;

[Trait("Category", "Integration")]
[Collection("Integration")]
public class CommentApiTests(IntegrationTestFixture fixture)
{
    private readonly HttpClient _client = fixture.HttpClient;

    [Fact]
    public async Task CreateComment_ReturnsCreated()
    {
        await _client.PostAsJsonAsync("/api/v1/spaces", new CreateSpaceRequest("CMT", "Comment Space", null));
        var pageResp = await _client.PostAsJsonAsync("/api/v1/spaces/CMT/pages", new CreatePageRequest("Page", "Content", null));
        var page = await pageResp.Content.ReadFromJsonAsync<ApiResponse<PageResponse>>();

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/spaces/CMT/pages/{page!.Items!.Id}/comments",
            new CreateCommentRequest("Hello", null, "author"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<CommentResponse>>();
        body!.Items!.Content.Should().Be("Hello");
    }

    [Fact]
    public async Task GetComments_ReturnsPaginatedList()
    {
        await _client.PostAsJsonAsync("/api/v1/spaces", new CreateSpaceRequest("CM2", "Comment Space 2", null));
        var pageResp = await _client.PostAsJsonAsync("/api/v1/spaces/CM2/pages", new CreatePageRequest("Page", "Content", null));
        var page = await pageResp.Content.ReadFromJsonAsync<ApiResponse<PageResponse>>();
        await _client.PostAsJsonAsync($"/api/v1/spaces/CM2/pages/{page!.Items!.Id}/comments",
            new CreateCommentRequest("Comment 1", null, null));

        var response = await _client.GetAsync($"/api/v1/spaces/CM2/pages/{page.Items.Id}/comments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedResponse<CommentResponse>>>();
        body!.Items!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateComment_ReturnsOk()
    {
        await _client.PostAsJsonAsync("/api/v1/spaces", new CreateSpaceRequest("CM3", "Comment Space 3", null));
        var pageResp = await _client.PostAsJsonAsync("/api/v1/spaces/CM3/pages", new CreatePageRequest("Page", "Content", null));
        var page = await pageResp.Content.ReadFromJsonAsync<ApiResponse<PageResponse>>();
        var createResp = await _client.PostAsJsonAsync($"/api/v1/spaces/CM3/pages/{page!.Items!.Id}/comments",
            new CreateCommentRequest("Original", null, null));
        var comment = await createResp.Content.ReadFromJsonAsync<ApiResponse<CommentResponse>>();

        var response = await _client.PutAsJsonAsync(
            $"/api/v1/spaces/CM3/pages/{page.Items.Id}/comments/{comment!.Items!.Id}",
            new UpdateCommentRequest("Updated"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<CommentResponse>>();
        body!.Items!.Content.Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteComment_ReturnsOk()
    {
        await _client.PostAsJsonAsync("/api/v1/spaces", new CreateSpaceRequest("CM4", "Comment Space 4", null));
        var pageResp = await _client.PostAsJsonAsync("/api/v1/spaces/CM4/pages", new CreatePageRequest("Page", "Content", null));
        var page = await pageResp.Content.ReadFromJsonAsync<ApiResponse<PageResponse>>();
        var createResp = await _client.PostAsJsonAsync($"/api/v1/spaces/CM4/pages/{page!.Items!.Id}/comments",
            new CreateCommentRequest("To delete", null, null));
        var comment = await createResp.Content.ReadFromJsonAsync<ApiResponse<CommentResponse>>();

        var response = await _client.DeleteAsync(
            $"/api/v1/spaces/CM4/pages/{page.Items.Id}/comments/{comment!.Items!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateComment_NonExistentPage_ReturnsNotFound()
    {
        await _client.PostAsJsonAsync("/api/v1/spaces", new CreateSpaceRequest("CM5", "Comment Space 5", null));

        var response = await _client.PostAsJsonAsync(
            "/api/v1/spaces/CM5/pages/99999/comments",
            new CreateCommentRequest("Hello", null, null));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
