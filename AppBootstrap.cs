using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.TestHost;
using RaccoltaASM.Services;

namespace RaccoltaASM;

public static class AppBootstrap
{
    public static WebApplication BuildApp(
        string[] args,
        Action<IServiceCollection>? configureServices = null,
        bool useTestServer = false)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Register services
        builder.Services.AddHttpClient();
        builder.Services.AddScoped<IAsmaqService, AsmaqService>();
        builder.Services.AddProblemDetails();

        configureServices?.Invoke(builder.Services);

        if (useTestServer)
        {
            builder.WebHost.UseTestServer();
        }

        var app = builder.Build();

        app.UseExceptionHandler(exceptionApp =>
        {
            exceptionApp.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

                if (exception is null)
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    return;
                }

                var (statusCode, title) = exception switch
                {
                    HttpRequestException => (StatusCodes.Status503ServiceUnavailable, "Servizio esterno non disponibile"),
                    JsonException => (StatusCodes.Status502BadGateway, "Risposta non valida dal servizio esterno"),
                    _ => (StatusCodes.Status500InternalServerError, "Errore interno del server")
                };

                context.Response.StatusCode = statusCode;

                await Results.Problem(
                    title: title,
                    statusCode: statusCode,
                    extensions: new Dictionary<string, object?>
                    {
                        ["traceId"] = context.TraceIdentifier
                    }).ExecuteAsync(context);
            });
        });

        // Endpoints
        app.MapGet("/", () =>
        {
            var assembly = typeof(Program).Assembly;
            var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? assembly.GetName().Version?.ToString()
                ?? "0.0.0";

            var today = AsmaqConstants.GetTodayInRome();
            var next7Days = today.AddDays(7);
            var raccoltaUrl = $"/raccolta?inizio={today:yyyy-MM-dd}&fine={next7Days:yyyy-MM-dd}";

            return Results.Ok(new
            {
                servizio = "RaccoltaASM",
                descrizione = "Servizio di calendario della raccolta rifiuti per L'Aquila (ASMAQ)",
                versione = version,
                endpoint = new
                {
                    raccolta = raccoltaUrl,
                    salute = "/health"
                }
            });
        })
            .WithName("Info");

        app.MapGet("/raccolta", GetRaccolta)
            .WithName("GetRaccolta")
            .Produces<IEnumerable<RaccoltaGroupDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
            .WithName("HealthCheck");

        return app;
    }

    private static async Task<IResult> GetRaccolta(
        IAsmaqService asmaqService,
        DateOnly? inizio = null,
        DateOnly? fine = null,
        CancellationToken cancellationToken = default)
    {
        if (inizio.HasValue && fine.HasValue && inizio > fine)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["fine"] = ["La data di fine non può essere precedente alla data di inizio"]
            }, title: "Intervallo date non valido");
        }

        var events = await asmaqService.GetRaccoltaAsync(inizio, fine, cancellationToken);
        return Results.Ok(events);
    }
}