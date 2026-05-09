using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using RaccoltaASM.Services;
using Xunit;

namespace RaccoltaASM.Tests;

public sealed class TestApplicationFactory : IAsyncLifetime
{
    private WebApplication? _application;

    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _application = RaccoltaASM.AppBootstrap.BuildApp(
            Array.Empty<string>(),
            services =>
            {
                var descriptor = services.SingleOrDefault(service => service.ServiceType == typeof(IAsmaqService));
                if (descriptor is not null)
                {
                    services.Remove(descriptor);
                }

                services.AddSingleton<IAsmaqService, FakeAsmaqService>();
            },
            useTestServer: true);

        await _application.StartAsync();
        Client = _application.GetTestClient();
    }

    public async Task DisposeAsync()
    {
        if (_application is not null)
        {
            await _application.DisposeAsync();
        }
    }
}

public sealed class FakeAsmaqService : IAsmaqService
{
    public Task<IEnumerable<RaccoltaGroupDto>> GetRaccoltaAsync(DateOnly? inizio = null, DateOnly? fine = null, CancellationToken cancellationToken = default)
    {
        IEnumerable<RaccoltaGroupDto> result =
        [
            new RaccoltaGroupDto("2026-05-09", "Carta, Plastica"),
            new RaccoltaGroupDto("2026-05-10", "Vetro")
        ];

        return Task.FromResult(result);
    }
}
