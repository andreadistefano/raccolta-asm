using System.Globalization;
using System.Text.Json;

namespace RaccoltaASM.Services;

/// <inheritdoc/>
public partial class AsmaqService : IAsmaqService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AsmaqService> _logger;

    public AsmaqService(IHttpClientFactory httpClientFactory, ILogger<AsmaqService> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<RaccoltaGroupDto>> GetRaccoltaAsync(DateOnly? inizio = null, DateOnly? fine = null, CancellationToken cancellationToken = default)
    {
        var today = AsmaqConstants.GetTodayInRome();
        var startDate = inizio ?? today;
        var endDate = fine ?? today.AddDays(AsmaqConstants.DefaultDateRangeDays);

        FetchingCollectionEvents(_logger, startDate, endDate);

        try
        {
            var url = AsmaqConstants.GetCalendarEventsUrl(startDate, endDate);
            var json = await FetchCalendarEventsAsync(url, cancellationToken);

            var data = JsonSerializer.Deserialize<AsmaqResponse>(json);
            var events = ProcessEvents(data, startDate, endDate);

            var grouped = GroupByDate(events);

            SuccessfullyFetchedAndGrouped(_logger, grouped.Count);

            return grouped;
        }
        catch (Exception ex)
        {
            ErrorFetchingCollectionEvents(_logger, ex);
            throw;
        }
    }

    /// <summary>
    /// Fetches raw calendar events JSON from ASMAQ API
    /// </summary>
    private async Task<string> FetchCalendarEventsAsync(string url, CancellationToken cancellationToken)
    {
        using var client = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        request.Headers.Add("X-Requested-With", "XMLHttpRequest");
        request.Headers.Add("User-Agent", "Mozilla/5.0");
        request.Headers.Add("Referer", $"{AsmaqConstants.BaseUrl}/comune-laquila-raccolta-e-trasporto/");

        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    /// <summary>
    /// Processes raw events and extracts matching dates within the range
    /// </summary>
    private static List<RaccoltaDto> ProcessEvents(AsmaqResponse? data, DateOnly startDate, DateOnly endDate)
    {
        var result = new List<RaccoltaDto>();

        if (data?.EVENTS is null)
            return result;

        foreach (var ev in data.EVENTS)
        {
            var titolo = ev.Titolo;
            var ricorrenze = ev.Ricorrenze ?? "";

            if (string.IsNullOrWhiteSpace(titolo))
                continue;

            foreach (var date in ExtractDates(ricorrenze))
            {
                if (date >= startDate && date <= endDate)
                {
                    result.Add(new RaccoltaDto(
                        date.ToString(AsmaqConstants.OutputDateFormat, CultureInfo.InvariantCulture),
                        titolo
                    ));
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Groups collection events by date and combines multiple events for the same day
    /// </summary>
    private static List<RaccoltaGroupDto> GroupByDate(IEnumerable<RaccoltaDto> events)
    {
        return events
            .GroupBy(x => x.Data)
            .Select(g => new RaccoltaGroupDto(
                g.Key,
                string.Join(", ", g.Select(x => x.Raccolta).Distinct())
            ))
                .OrderBy(x => DateOnly.ParseExact(x.Data, AsmaqConstants.OutputDateFormat, CultureInfo.InvariantCulture))
            .ToList();
    }

    /// <summary>
    /// Extracts dates from the ricorrenze field (comma-separated ISO 8601 dates)
    /// </summary>
    private static IEnumerable<DateOnly> ExtractDates(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            yield break;

        var parts = raw.Split(',');

        foreach (var p in parts)
        {
            var clean = p.Split('/')[0]; // Format: 20260509T000000

            if (clean.Length < 8)
                continue;

            if (DateOnly.TryParseExact(
                clean[..8],
                AsmaqConstants.DateTimeFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var dt))
            {
                yield return dt;
            }
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Fetching collection events from {Start} to {End}")]
    private static partial void FetchingCollectionEvents(ILogger logger, DateOnly start, DateOnly end);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Successfully fetched and grouped {Count} collection events")]
    private static partial void SuccessfullyFetchedAndGrouped(ILogger logger, int count);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Error fetching collection events")]
    private static partial void ErrorFetchingCollectionEvents(ILogger logger, Exception exception);
}
