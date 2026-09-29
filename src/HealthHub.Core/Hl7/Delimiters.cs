namespace HealthHub.Core.Hl7;
//reads encoding chars from MSH instead of assuming them
public sealed record Delimiters(char Field, char Component, char Repetition, char Escape, char Subcomponent) 
{
    public static Delimiters FromMsh(string mshSegment)
    {
        //"MSH" + field separator +4 encoding chars = 8 char

        if (mshSegment.Length < 8 || !mshSegment.StartsWith("MSH"))
            throw new FormatException("Message must begin with an MSH segment.");

            return new Delimiters(
                Field: mshSegment[3],
                Component: mshSegment[4],
                Repetition: mshSegment[5], 
                Escape: mshSegment[6],
                Subcomponent: mshSegment[7]
            );
    }
}
