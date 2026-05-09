using System.Text.Json.Serialization;

namespace RaccoltaASM.Models;

/// <summary>
/// Response model from ASMAQ calendar API
/// </summary>
public class AsmaqResponse
{
    public List<AsmaqEvent>? EVENTS { get; set; }
}

/// <summary>
/// Represents a single event from ASMAQ calendar
/// </summary>
public class AsmaqEvent
{
    /// <summary>
    /// Event title (e.g., "Raccolta Carta")
    /// </summary>
    [JsonPropertyName("3")]
    public string? Titolo { get; set; }

    /// <summary>
    /// Event occurrences in ISO 8601 format (comma-separated)
    /// </summary>
    [JsonPropertyName("14")]
    public string? Ricorrenze { get; set; }
}
