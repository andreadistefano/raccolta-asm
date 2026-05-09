using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace RaccoltaASM.Tests;

public class ApiIntegrationTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient _client;

    public ApiIntegrationTests(TestApplicationFactory factory)
    {
        _client = factory.Client;
    }

    [Fact]
    public async Task HealthEndpointReturnsOk()
    {
        var response = await _client.GetAsync("/health");

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

        Assert.NotNull(body);
        Assert.Equal("healthy", body!["status"]);
    }

    [Fact]
    public async Task RaccoltaEndpointReturnsGroupedEvents()
    {
        var response = await _client.GetAsync("/raccolta?inizio=2026-05-09&fine=2026-05-10");

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<List<Dictionary<string, string>>>();

        Assert.NotNull(body);
        Assert.Equal(2, body!.Count);
        Assert.Equal("2026-05-09", body[0]["data"]);
        Assert.Equal("Carta, Plastica", body[0]["raccolta"]);
        Assert.Equal("2026-05-10", body[1]["data"]);
        Assert.Equal("Vetro", body[1]["raccolta"]);
    }

    [Fact]
    public async Task RaccoltaEndpointReturnsValidationProblemForInvalidRange()
    {
        var response = await _client.GetAsync("/raccolta?inizio=2026-05-10&fine=2026-05-09");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<JsonDocument>();

        Assert.NotNull(problem);
        Assert.Equal("Intervallo date non valido", problem!.RootElement.GetProperty("title").GetString());
    }
}
