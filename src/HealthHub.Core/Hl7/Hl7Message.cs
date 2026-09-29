namespace HealthHub.Core.Hl7;

public sealed class Hl7Message
{
    public Delimiters Delimiters {get; }
    public IReadOnlyList<Segment> Segments {get;}

    private Hl7Message(Delimiters delimiters, IReadOnlyList<Segment>segments)
    {
        Delimiters = delimiters;
        Segments = segments;

    }
    public static Hl7Message Parse(string raw)
    {
        //Real HL7 uses /r only. This mock accepts /n /\r\n to allow pasted text.
        //production show log a warning when it seems it

        var lines = raw.Replace("\r\n", "\r").Replace('\n', '\r').Split('\r',StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length == 0)
            throw new FormatException("Message is empty");

        var delims = Delimiters.FromMsh(lines[0]);
        var segments = lines.Select(line => new Segment(line, delims)).ToList();
        return new Hl7Message(delims, segments);
            }
        
    public Segment? GetSegment(string name) => Segments.FirstOrDefault(s => s.Name == name);

    //shortcuts for freq used fields
    public string MessageType  => GetSegment("MSH")?.GetField(9) ?? "";
    public string ControlId => GetSegment("MSH")?.GetField(10) ?? "";

}