using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HobomSpace.Api.Contracts;

namespace HobomSpace.Tests.Integration;

[Trait("Category", "Integration")]
[Collection("Integration")]
public class PageApiTests(IntegrationTestFixture fixture)
{
    private readonly HttpClient _client = fixture.HttpClient;

    [Fact]
    public async Task CreatePage_ReturnsCreated()
    {
        await _client.PostAsJsonAsync("/api/v1/spaces", new CreateSpaceRequest("PG", "Page Space", null));

        var response = await _client.PostAsJsonAsync(
            "/api/v1/spaces/PG/pages",
            new CreatePageRequest("Test Page", "Content", null));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<PageResponse>();
        body!.Title.Should().Be("Test Page");
    }

    [Fact]
    public async Task CreatePage_WithNonExistentSpace_ReturnsNotFound()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/spaces/NOEXIST/pages",
            new CreatePageRequest("Title", "Content", null));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPages_ReturnsTreeStructure()
    {
        await _client.PostAsJsonAsync("/api/v1/spaces", new CreateSpaceRequest("TREE", "Tree Space", null));
        await _client.PostAsJsonAsync("/api/v1/spaces/TREE/pages", new CreatePageRequest("Root", "Content", null));

        var response = await _client.GetAsync("/api/v1/spaces/TREE/pages");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdatePage_ReturnsOk()
    {
        await _client.PostAsJsonAsync("/api/v1/spaces", new CreateSpaceRequest("PGUP", "Update Space", null));
        var createResponse = await _client.PostAsJsonAsync(
            "/api/v1/spaces/PGUP/pages",
            new CreatePageRequest("Original", "Content", null));
        var created = await createResponse.Content.ReadFromJsonAsync<PageResponse>();

        var response = await _client.PutAsJsonAsync(
            $"/api/v1/spaces/PGUP/pages/{created!.Id}",
            new UpdatePageRequest("Updated", "New Content", null));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PageResponse>();
        body!.Title.Should().Be("Updated");
    }

    [Fact]
    public async Task DeletePage_ReturnsNoContent()
    {
        await _client.PostAsJsonAsync("/api/v1/spaces", new CreateSpaceRequest("PGDL", "Delete Space", null));
        var createResponse = await _client.PostAsJsonAsync(
            "/api/v1/spaces/PGDL/pages",
            new CreatePageRequest("To Delete", "Content", null));
        var created = await createResponse.Content.ReadFromJsonAsync<PageResponse>();

        var response = await _client.DeleteAsync($"/api/v1/spaces/PGDL/pages/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
