namespace HealthHub.Core.Hl7;

///The outcome of validating one inbound msg

public sealed record ValidationResult(
    string AckCode,  //AR,AE,AA
    int ErrorCode = 0, //Hl7 table 0357
    string? ErrorCodeText = null, //e.g. "Required field missing"
    string? Segment = null, //where problem is such as PID
    int? Field = null,
    string? ErrorText = null) //readable explanation
{
    public static readonly ValidationResult Accept = new("AA");
    public bool isAccepted => AckCode == "AA";
}
