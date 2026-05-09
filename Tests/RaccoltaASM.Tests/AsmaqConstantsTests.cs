using RaccoltaASM.Constants;
using Xunit;

namespace RaccoltaASM.Tests;

public class AsmaqConstantsTests
{
    [Fact]
    public void GetCalendarEventsUrlUsesExpectedQueryParameters()
    {
        var inizio = new DateOnly(2026, 5, 9);
        var fine = new DateOnly(2026, 5, 10);

        var url = AsmaqConstants.GetCalendarEventsUrl(inizio, fine);

        Assert.Contains("rhc_action=get_calendar_events", url);
        Assert.Contains("post_type[]=events", url);
        Assert.Contains("start=", url);
        Assert.Contains("end=", url);
        Assert.Contains("rhc_shrink=1", url);
        Assert.Contains("view=month", url);
    }

    [Fact]
    public void GetTodayInRomeReturnsAValidDateOnly()
    {
        var today = AsmaqConstants.GetTodayInRome();

        Assert.True(today.Year >= 2024);
    }
}
