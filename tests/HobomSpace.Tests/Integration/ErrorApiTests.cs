using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HobomSpace.Api.Contracts;

namespace HobomSpace.Tests.Integration;

[Trait("Category", "Integration")]
[Collection("Integration")]
public class ErrorApiTests(IntegrationTestFixture fixture)
{
    private readonly HttpClient _client = fixture.HttpClient;

    [Fact]
    public async Task CaptureError_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/errors",
            new CaptureErrorRequest("Test error", "stack trace", "/home", "CLIENT_LOGIC", "Chrome"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ErrorEventResponse>>();
        body!.Items!.Message.Should().Be("Test error");
    }

    [Fact]
    public async Task GetErrors_ReturnsOk()
    {
        await _client.PostAsJsonAsync("/api/v1/errors",
            new CaptureErrorRequest("List error", null, "/list", "CLIENT_LOGIC", null));

        var response = await _client.GetAsync("/api/v1/errors");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedResponse<ErrorEventResponse>>>();
        body!.Items!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetErrorById_ReturnsOk()
    {
        var createResp = await _client.PostAsJsonAsync("/api/v1/errors",
            new CaptureErrorRequest("Detail error", "trace", "/detail", "CLIENT_LOGIC", "Firefox"));
        var error = await createResp.Content.ReadFromJsonAsync<ApiResponse<ErrorEventResponse>>();

        var response = await _client.GetAsync($"/api/v1/errors/{error!.Items!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ErrorEventResponse>>();
        body!.Items!.Message.Should().Be("Detail error");
    }
}
