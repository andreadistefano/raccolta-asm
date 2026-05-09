using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace RaccoltaASM.Tests;

public class RootEndpointTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient _client;

    public RootEndpointTests(TestApplicationFactory factory)
    {
        _client = factory.Client;
    }

    [Fact]
    public async Task RootEndpointReturnsServiceMetadata()
    {
        var response = await _client.GetAsync("/");

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<JsonDocument>();

        Assert.NotNull(body);
        var root = body!.RootElement;

        Assert.Equal("RaccoltaASM", root.GetProperty("servizio").GetString());
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("versione").GetString()));

        var endpoint = root.GetProperty("endpoint");
        var raccolta = endpoint.GetProperty("raccolta").GetString();
        var salute = endpoint.GetProperty("salute").GetString();

        Assert.NotNull(raccolta);
        Assert.Contains("/raccolta?inizio=", raccolta!);
        Assert.Contains("&fine=", raccolta!);
        Assert.Equal("/health", salute);
    }
}
