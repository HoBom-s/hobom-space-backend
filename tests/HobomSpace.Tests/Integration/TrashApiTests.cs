using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HobomSpace.Api.Contracts;

namespace HobomSpace.Tests.Integration;

[Trait("Category", "Integration")]
[Collection("Integration")]
public class TrashApiTests(IntegrationTestFixture fixture)
{
    private readonly HttpClient _client = fixture.HttpClient;

    [Fact]
    public async Task GetTrash_AfterDelete_ReturnsDeletedPage()
    {
        await _client.PostAsJsonAsync("/api/v1/spaces", new CreateSpaceRequest("TRS", "Trash Space", null));
        var pageResp = await _client.PostAsJsonAsync("/api/v1/spaces/TRS/pages",
            new CreatePageRequest("Trashed", "Content", null));
        var page = await pageResp.Content.ReadFromJsonAsync<ApiResponse<PageResponse>>();
        await _client.DeleteAsync($"/api/v1/spaces/TRS/pages/{page!.Items!.Id}");

        var response = await _client.GetAsync("/api/v1/spaces/TRS/trash");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedResponse<TrashPageResponse>>>();
        body!.Items!.Items.Should().Contain(p => p.Title == "Trashed");
    }

    [Fact]
    public async Task RestoreFromTrash_ReturnsOk()
    {
        await _client.PostAsJsonAsync("/api/v1/spaces", new CreateSpaceRequest("TR2", "Trash Space 2", null));
        var pageResp = await _client.PostAsJsonAsync("/api/v1/spaces/TR2/pages",
            new CreatePageRequest("Restore Me", "Content", null));
        var page = await pageResp.Content.ReadFromJsonAsync<ApiResponse<PageResponse>>();
        await _client.DeleteAsync($"/api/v1/spaces/TR2/pages/{page!.Items!.Id}");

        var response = await _client.PostAsync($"/api/v1/spaces/TR2/trash/{page.Items.Id}/restore", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PermanentDelete_ReturnsOk()
    {
        await _client.PostAsJsonAsync("/api/v1/spaces", new CreateSpaceRequest("TR3", "Trash Space 3", null));
        var pageResp = await _client.PostAsJsonAsync("/api/v1/spaces/TR3/pages",
            new CreatePageRequest("Perm Delete", "Content", null));
        var page = await pageResp.Content.ReadFromJsonAsync<ApiResponse<PageResponse>>();
        await _client.DeleteAsync($"/api/v1/spaces/TR3/pages/{page!.Items!.Id}");

        var response = await _client.DeleteAsync($"/api/v1/spaces/TR3/trash/{page.Items.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
