using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HobomSpace.Api.Contracts;

namespace HobomSpace.Tests.Integration;

[Trait("Category", "Integration")]
[Collection("Integration")]
public class LabelApiTests(IntegrationTestFixture fixture)
{
    private readonly HttpClient _client = fixture.HttpClient;

    [Fact]
    public async Task CreateLabel_ReturnsCreated()
    {
        await _client.PostAsJsonAsync("/api/v1/spaces", new CreateSpaceRequest("LBL", "Label Space", null));

        var response = await _client.PostAsJsonAsync("/api/v1/spaces/LBL/labels",
            new CreateLabelRequest("Bug", "#FF0000"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<LabelResponse>>();
        body!.Items!.Name.Should().Be("Bug");
    }

    [Fact]
    public async Task GetLabels_ReturnsOk()
    {
        await _client.PostAsJsonAsync("/api/v1/spaces", new CreateSpaceRequest("LB2", "Label Space 2", null));
        await _client.PostAsJsonAsync("/api/v1/spaces/LB2/labels", new CreateLabelRequest("Feature", "#00FF00"));

        var response = await _client.GetAsync("/api/v1/spaces/LB2/labels");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<LabelResponse>>>();
        body!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateLabel_ReturnsOk()
    {
        await _client.PostAsJsonAsync("/api/v1/spaces", new CreateSpaceRequest("LB3", "Label Space 3", null));
        var createResp = await _client.PostAsJsonAsync("/api/v1/spaces/LB3/labels",
            new CreateLabelRequest("Old", "#000000"));
        var label = await createResp.Content.ReadFromJsonAsync<ApiResponse<LabelResponse>>();

        var response = await _client.PutAsJsonAsync($"/api/v1/spaces/LB3/labels/{label!.Items!.Id}",
            new UpdateLabelRequest("New", "#FFFFFF"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<LabelResponse>>();
        body!.Items!.Name.Should().Be("New");
    }

    [Fact]
    public async Task DeleteLabel_ReturnsOk()
    {
        await _client.PostAsJsonAsync("/api/v1/spaces", new CreateSpaceRequest("LB4", "Label Space 4", null));
        var createResp = await _client.PostAsJsonAsync("/api/v1/spaces/LB4/labels",
            new CreateLabelRequest("ToDelete", "#000000"));
        var label = await createResp.Content.ReadFromJsonAsync<ApiResponse<LabelResponse>>();

        var response = await _client.DeleteAsync($"/api/v1/spaces/LB4/labels/{label!.Items!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AddLabelToPage_ReturnsCreated()
    {
        await _client.PostAsJsonAsync("/api/v1/spaces", new CreateSpaceRequest("LB5", "Label Space 5", null));
        var labelResp = await _client.PostAsJsonAsync("/api/v1/spaces/LB5/labels",
            new CreateLabelRequest("Attach", "#123456"));
        var label = await labelResp.Content.ReadFromJsonAsync<ApiResponse<LabelResponse>>();
        var pageResp = await _client.PostAsJsonAsync("/api/v1/spaces/LB5/pages",
            new CreatePageRequest("Page", "Content", null));
        var page = await pageResp.Content.ReadFromJsonAsync<ApiResponse<PageResponse>>();

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/spaces/LB5/pages/{page!.Items!.Id}/labels",
            new AddPageLabelRequest(label!.Items!.Id));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task RemoveLabelFromPage_ReturnsOk()
    {
        await _client.PostAsJsonAsync("/api/v1/spaces", new CreateSpaceRequest("LB6", "Label Space 6", null));
        var labelResp = await _client.PostAsJsonAsync("/api/v1/spaces/LB6/labels",
            new CreateLabelRequest("Remove", "#654321"));
        var label = await labelResp.Content.ReadFromJsonAsync<ApiResponse<LabelResponse>>();
        var pageResp = await _client.PostAsJsonAsync("/api/v1/spaces/LB6/pages",
            new CreatePageRequest("Page", "Content", null));
        var page = await pageResp.Content.ReadFromJsonAsync<ApiResponse<PageResponse>>();
        await _client.PostAsJsonAsync($"/api/v1/spaces/LB6/pages/{page!.Items!.Id}/labels",
            new AddPageLabelRequest(label!.Items!.Id));

        var response = await _client.DeleteAsync(
            $"/api/v1/spaces/LB6/pages/{page.Items.Id}/labels/{label.Items.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
