using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic;

namespace HealthHub.Core.Hl7;

public static class AckBuilder
{
    public static string Build(Hl7Message original, ValidationResult result, string ackControlId, DateTimeOffset now)
    {
        var d = original.Delimiters;
        var msh = original.GetSegment("MSH")!;
        string[] mshFields = 
        [ "MSH", msh.GetField(2), msh.GetField(5), msh.GetField(6), msh.GetField(3), msh.GetField(4), FormatTimestamp(now),"", string.Join(d.Component, "ACK", msh.GetComponent(9,2), "ACK"), ackControlId, msh.GetField(11), msh.GetField(12),    
        ];

        var segments = new List<string>
        {
            string.Join(d.Field, mshFields),
            string.Join(d.Field, "MSA", result.AckCode, original.ControlId),
        };
        if (!result.IsAccepted)
            segments.Add(BuildErr(result,d));
        return string.Join('\r', segments);
    }
    private static string BuildErr(ValidationResult r, Delimiters d)
    {
        var fields = new string[9];
        fields[0] = "ERR";
        if (r.Segment is not null)
            fields[2] = string.Join(d.Component, r.Segment, "1", r.Field?.ToString() ?? ""); 
            fields[3] = string.Join(d.Component, r.ErrorCode, r.ErrorCodeText, "HL70357"); 
            fields[4] = "E"; //severity
            fields[8] = Hl7Escaping.Escape(r.ErrorText ?? "", d); //user message
        return string.Join(d.Field, fields);

    }
    private static string FormatTimestamp(DateTimeOffset t)
    {
        string sign = t.Offset < TimeSpan.Zero ? "-" :"+";
        return t.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) + sign + t.Offset.Duration().ToString("hhmm", CultureInfo.InvariantCulture);

    }
}