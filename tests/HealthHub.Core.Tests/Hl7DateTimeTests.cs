using HealthHub.Core.Hl7;

namespace HealthHub.Core.Tests;

public class Hl7DateTimeTests
{
    [Theory]
    [InlineData("20261001090000", 2026, 10, 1, 9, 0, 0)]
    [InlineData("202610010900", 2026, 10, 1, 9, 0, 0)]
    [InlineData("20261001", 2026, 10, 1, 0, 0, 0)]
    [InlineData("20261001090000-0400", 2026, 10, 1, 9, 0, 0)]
    [InlineData("20261001090000.1234+0100", 2026, 10, 1, 9, 0, 0)]
    public void Parses_valid_dates(string input, int year, int month, int day, int hour, int minute, int second)
    {
        var expected = new DateTime(year, month, day, hour, minute, second);

        Assert.Equal(expected, Hl7DateTime.Parse(input));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("20261345")]
    [InlineData("not a date")]
    [InlineData("2026")]
    public void Returns_null_for_empty_or_invalid(string input)
    {
        Assert.Null(Hl7DateTime.Parse(input));
    }
}