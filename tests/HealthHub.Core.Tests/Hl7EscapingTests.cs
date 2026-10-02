using System.Configuration.Assemblies;
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
    
    [Theory]
    [InlineData("Smith & Jones", "Smith \\T\\ Jones")]
    [InlineData("A|B", "A\\F\\B")]
    [InlineData(@"C:\temp", "C:\\E\\temp")]
    [InlineData("No special characters", "No special characters")]
    
    public void Escape_makes_text_safe_for_a_field( string input, string expected)
    {
        Assert.Equal(expected, Hl7Escaping.Escape(input, Std));
    }

    [Theory]
    [InlineData("Smith & Jones")]
    [InlineData("Hemolyzed | recollect")]
    [InlineData(@"\T\ is how you write an ampersand")]
    [InlineData("^~\\&|")]

    public void Escape_then_unescape_returns_the_original(string original)
    {
    string escaped = Hl7Escaping.Escape(original, Std);

    Assert.Equal(original, Hl7Escaping.Unescape(escaped, Std));
    }
}