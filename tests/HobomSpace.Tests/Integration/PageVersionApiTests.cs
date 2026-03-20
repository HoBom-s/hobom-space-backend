using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HobomSpace.Api.Contracts;

namespace HobomSpace.Tests.Integration;

[Trait("Category", "Integration")]
[Collection("Integration")]
public class PageVersionApiTests(IntegrationTestFixture fixture)
{
    private readonly HttpClient _client = fixture.HttpClient;

    [Fact]
    public async Task GetHistory_AfterUpdate_ReturnsVersions()
    {
        await _client.PostAsJsonAsync("/api/v1/spaces", new CreateSpaceRequest("VER", "Version Space", null));
        var pageResp = await _client.PostAsJsonAsync("/api/v1/spaces/VER/pages",
            new CreatePageRequest("V1 Title", "V1 Content", null));
        var page = await pageResp.Content.ReadFromJsonAsync<ApiResponse<PageResponse>>();
        await _client.PutAsJsonAsync($"/api/v1/spaces/VER/pages/{page!.Items!.Id}",
            new UpdatePageRequest("V2 Title", "V2 Content", null));

        var response = await _client.GetAsync($"/api/v1/spaces/VER/pages/{page.Items.Id}/versions");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedResponse<PageVersionResponse>>>();
        body!.Items!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetSpecificVersion_ReturnsOk()
    {
        await _client.PostAsJsonAsync("/api/v1/spaces", new CreateSpaceRequest("VR2", "Version Space 2", null));
        var pageResp = await _client.PostAsJsonAsync("/api/v1/spaces/VR2/pages",
            new CreatePageRequest("Title", "Content", null));
        var page = await pageResp.Content.ReadFromJsonAsync<ApiResponse<PageResponse>>();
        await _client.PutAsJsonAsync($"/api/v1/spaces/VR2/pages/{page!.Items!.Id}",
            new UpdatePageRequest("Updated", "New", null));

        var response = await _client.GetAsync($"/api/v1/spaces/VR2/pages/{page.Items.Id}/versions/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PageVersionResponse>>();
        body!.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task RestoreVersion_ReturnsOk()
    {
        await _client.PostAsJsonAsync("/api/v1/spaces", new CreateSpaceRequest("VR3", "Version Space 3", null));
        var pageResp = await _client.PostAsJsonAsync("/api/v1/spaces/VR3/pages",
            new CreatePageRequest("Original", "Original Content", null));
        var page = await pageResp.Content.ReadFromJsonAsync<ApiResponse<PageResponse>>();
        await _client.PutAsJsonAsync($"/api/v1/spaces/VR3/pages/{page!.Items!.Id}",
            new UpdatePageRequest("Changed", "Changed Content", null));

        var response = await _client.PostAsync(
            $"/api/v1/spaces/VR3/pages/{page.Items.Id}/versions/1/restore", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetDiff_ReturnsOk()
    {
        await _client.PostAsJsonAsync("/api/v1/spaces", new CreateSpaceRequest("VR4", "Version Space 4", null));
        var pageResp = await _client.PostAsJsonAsync("/api/v1/spaces/VR4/pages",
            new CreatePageRequest("Diff Title", "Line1\nLine2", null));
        var page = await pageResp.Content.ReadFromJsonAsync<ApiResponse<PageResponse>>();
        // first update → creates version 1 snapshot
        await _client.PutAsJsonAsync($"/api/v1/spaces/VR4/pages/{page!.Items!.Id}",
            new UpdatePageRequest("Diff Title", "Line1\nLine3", null));
        // second update → creates version 2 snapshot
        await _client.PutAsJsonAsync($"/api/v1/spaces/VR4/pages/{page.Items.Id}",
            new UpdatePageRequest("Diff Title V3", "Line1\nLine4", null));

        var response = await _client.GetAsync(
            $"/api/v1/spaces/VR4/pages/{page.Items.Id}/versions/diff?from=1&to=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
