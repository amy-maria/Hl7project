using HealthHub.Core.Hl7;

namespace HealthHub.Core.Tests;

public class Hl7EscapingTests
{
    // Standard HL7 delimiters. '\\' is how you write a single backslash character in C#.
    private static readonly Delimiters Std = new('|', '^', '~', '\\', '&');

    [Theory]//test method run with many inputs
    [InlineData(@"Smith \T\ Jones", "Smith & Jones")]//inlinedata runs as its own tests 
    [InlineData(@"A\F\B", "A|B")]
    [InlineData(@"A\S\B", "A^B")]
    [InlineData(@"A\R\B", "A~B")]
    [InlineData(@"C:\E\temp", @"C:\temp")]
    [InlineData(@"\E\T\E\", @"\T\")]
    [InlineData(@"Line one\.br\Line two", @"Line one\.br\Line two")]
    [InlineData("No escapes here", "No escapes here")]
    public void Unescape_converts_sequences(string input, string expected)
    {
        Assert.Equal(expected, Hl7Escaping.Unescape(input, Std));
    }
}