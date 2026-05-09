namespace RaccoltaASM.Services;

/// <summary>
/// Service for fetching and processing ASMAQ waste collection data
/// </summary>
public interface IAsmaqService
{
    /// <summary>
    /// Gets grouped waste collection events for the specified date range
    /// </summary>
    /// <param name="inizio">Start date (defaults to today in Rome)</param>
    /// <param name="fine">End date (defaults to 7 days from today in Rome)</param>
    /// <returns>Collection events grouped by date</returns>
    Task<IEnumerable<RaccoltaGroupDto>> GetRaccoltaAsync(DateOnly? inizio = null, DateOnly? fine = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Grouped collection data by date
/// </summary>
public record RaccoltaGroupDto(string Data, string Raccolta);
