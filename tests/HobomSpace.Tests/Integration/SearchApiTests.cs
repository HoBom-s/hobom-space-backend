using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HobomSpace.Api.Contracts;

namespace HobomSpace.Tests.Integration;

[Trait("Category", "Integration")]
[Collection("Integration")]
public class SearchApiTests(IntegrationTestFixture fixture)
{
    private readonly HttpClient _client = fixture.HttpClient;

    [Fact]
    public async Task SearchGlobal_ReturnsOk()
    {
        await _client.PostAsJsonAsync("/api/v1/spaces", new CreateSpaceRequest("SRC", "Search Space", null));
        await _client.PostAsJsonAsync("/api/v1/spaces/SRC/pages",
            new CreatePageRequest("Searchable Page", "findme content", null));

        var response = await _client.GetAsync("/api/v1/search?q=Searchable");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SearchInSpace_ReturnsOk()
    {
        await _client.PostAsJsonAsync("/api/v1/spaces", new CreateSpaceRequest("SR2", "Search Space 2", null));
        await _client.PostAsJsonAsync("/api/v1/spaces/SR2/pages",
            new CreatePageRequest("Space Search", "content", null));

        var response = await _client.GetAsync("/api/v1/search/spaces/SR2?q=Space");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SearchEmpty_ReturnsEmptyList()
    {
        var response = await _client.GetAsync("/api/v1/search?q=xyznonexistent99999");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedResponse<SearchResult>>>();
        body!.Items!.Items.Should().BeEmpty();
    }
}
