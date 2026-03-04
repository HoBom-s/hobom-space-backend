using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HobomSpace.Api.Contracts;

namespace HobomSpace.Tests.Integration;

[Trait("Category", "Integration")]
[Collection("Integration")]
public class SpaceApiTests(IntegrationTestFixture fixture)
{
    private readonly HttpClient _client = fixture.HttpClient;

    [Fact]
    public async Task CreateSpace_ReturnsCreated()
    {
        var request = new CreateSpaceRequest("INT", "Integration", "Test space");

        var response = await _client.PostAsJsonAsync("/api/v1/spaces", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<SpaceResponse>();
        body!.Key.Should().Be("INT");
        body.Name.Should().Be("Integration");
    }

    [Fact]
    public async Task CreateDuplicateSpace_ReturnsConflict()
    {
        var request = new CreateSpaceRequest("DUP", "Duplicate", null);
        await _client.PostAsJsonAsync("/api/v1/spaces", request);

        var response = await _client.PostAsJsonAsync("/api/v1/spaces", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetAllSpaces_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/v1/spaces");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSpace_WithNonExistentKey_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/v1/spaces/NOEXIST");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateSpace_ReturnsOk()
    {
        await _client.PostAsJsonAsync("/api/v1/spaces", new CreateSpaceRequest("UPD", "Original", null));

        var response = await _client.PutAsJsonAsync("/api/v1/spaces/UPD", new UpdateSpaceRequest("Updated", "New desc"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<SpaceResponse>();
        body!.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteSpace_ReturnsNoContent()
    {
        await _client.PostAsJsonAsync("/api/v1/spaces", new CreateSpaceRequest("DEL", "To Delete", null));

        var response = await _client.DeleteAsync("/api/v1/spaces/DEL");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RequestWithoutApiKey_ReturnsUnauthorized()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/api/v1/spaces");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task HealthCheck_BypassesApiKey()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/health/live");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
