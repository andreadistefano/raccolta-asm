namespace RaccoltaASM.Constants;

/// <summary>
/// Constants for ASMAQ API integration
/// </summary>
public static class AsmaqConstants
{
    public const string BaseUrl = "https://www.asmaq.it";
    public const string CalendarEventsPath = "/?rhc_action=get_calendar_events";

    public const string EventPropertyTitolo = "3";
    public const string EventPropertyRicorrenze = "14";

    public const string DateTimeFormat = "yyyyMMdd";
    public const string OutputDateFormat = "yyyy-MM-dd";

    /// <summary>
    /// Default number of days for the date range if end date is not specified
    /// </summary>
    public const int DefaultDateRangeDays = 7;

    public static DateOnly GetTodayInRome()
    {
        var timeZone = GetRomeTimeZone();
        var nowInRome = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone);
        return DateOnly.FromDateTime(nowInRome.DateTime);
    }

    public static string GetCalendarEventsUrl(DateOnly start, DateOnly end)
    {
        var startTs = GetRomeStartOfDay(start).ToUnixTimeSeconds();
        var endTs = GetRomeEndOfDay(end).ToUnixTimeSeconds();

        return $"{BaseUrl}{CalendarEventsPath}" +
               $"&post_type[]=events" +
               $"&start={startTs}" +
               $"&end={endTs}" +
               $"&rhc_shrink=1" +
               $"&view=month";
    }

    private static DateTimeOffset GetRomeStartOfDay(DateOnly date)
    {
        var timeZone = GetRomeTimeZone();
        var localDateTime = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return new DateTimeOffset(localDateTime, timeZone.GetUtcOffset(localDateTime));
    }

    private static DateTimeOffset GetRomeEndOfDay(DateOnly date)
    {
        var timeZone = GetRomeTimeZone();
        var localDateTime = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Unspecified);
        return new DateTimeOffset(localDateTime, timeZone.GetUtcOffset(localDateTime));
    }

    private static TimeZoneInfo GetRomeTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Rome");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.Utc;
        }
    }
}
