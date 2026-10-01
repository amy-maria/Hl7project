using HealthHub.Core.Hl7;

namespace HealthHub.Data;

///One row of dbo.Messages. Property names match the column names exactly.
/// 
public sealed class MessageRecord
{
    public long MessageId { get; set; }
    public int InterfaceId { get; set; }
    public string? SendingApplication { get; set; }
    public string? SendingFacility { get; set; }
    public string? ReceivingApplication { get; set; }
    public string? ReceivingFacility { get; set; }
    public DateTime? MessageDateTime { get; set; }
    public string MessageType { get; set; } = "";
    public string? TriggerEvent { get; set; }
    public string? MessageStructure { get; set; }
    public string ControlId { get; set; } = "";
    public string? ProcessingId { get; set; }
    public string? VersionId { get; set; }
    public string? PatientMrn { get; set; }
    public string RawMessage { get; set; } = "";
    public DateTime ReceivedAtUtc { get; set; }
    public string Status { get; set; } = "Received";
    public string? AckCode { get; set; }
    public string? ErrorText { get; set; }

    /// <summary>Builds a row from a parsed message, pulling the searchable fields out of MSH and PID.</summary>
    public static MessageRecord FromHl7(Hl7Message msg, string raw, int interfaceId)
    {
        var msh = msg.GetSegment("MSH")!;   // Parse() guarantees MSH exists

        return new MessageRecord
        {
            InterfaceId          = interfaceId,
            SendingApplication   = NullIfEmpty(msh.GetComponent(3, 1)),
            SendingFacility      = NullIfEmpty(msh.GetComponent(4, 1)),
            ReceivingApplication = NullIfEmpty(msh.GetComponent(5, 1)),
            ReceivingFacility    = NullIfEmpty(msh.GetComponent(6, 1)),
            MessageDateTime      = Hl7DateTime.Parse(msh.GetField(7)),
            MessageType          = msh.GetComponent(9, 1),
            TriggerEvent         = NullIfEmpty(msh.GetComponent(9, 2)),
            MessageStructure     = NullIfEmpty(msh.GetComponent(9, 3)),
            ControlId            = msh.GetField(10),
            ProcessingId         = NullIfEmpty(msh.GetComponent(11, 1)),
            VersionId            = NullIfEmpty(msh.GetComponent(12, 1)),
            PatientMrn           = FindMrn(msg),
            RawMessage           = raw,
        };
    }

    // Looks through the repetitions of PID-3 for the one whose ID type (CX.5) is "MR".
    private static string? FindMrn(Hl7Message msg)
    {
        var pid = msg.GetSegment("PID");
        if (pid is null)
            return null;

        foreach (string repetition in pid.GetRepetitions(3))
        {
            string[] cx = repetition.Split(msg.Delimiters.Component);
            if (cx.Length >= 5 && cx[4] == "MR")
                return NullIfEmpty(cx[0]);
        }
        return null;
    }

    private static string? NullIfEmpty(string value) => value == "" ? null : value;
}