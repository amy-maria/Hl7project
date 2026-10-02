using System.Data;

namespace HealthHub.Core.Hl7;

public static class MessageValidator
{
    private static readonly HashSet<string> SupportedTypes = ["ADT", "ORM", "OML", "ORU", "SIU"];

    public static ValidationResult Validate(Hl7Message msg, string expectedProcessingId = "P")
    {
        var msh = msg.GetSegment("MSH")!; //Parse() guarantees MSH exists
        //If header problems: AR reject and don't process it
        if (msg.ControlId == "")
            return Reject(101, "Required Field Missing", "MSH", 10, "MSH-10 (Message Control ID) is required");
        
        string processingId = msh.GetComponent(11, 1);
        if (processingId != expectedProcessingId)
            return Reject(202, "Unsupported processing id", "MSH", 11, $"Processing ID '{processingId}' not accepted; this receiver expect '{expectedProcessingId}'");
        
        string version = msh.GetComponent(12,1);
        if (!version.StartsWith("2."))
            return Reject(203, "Unsupported Version id", "MSH", 12, $"Version '{version}' is not supported");

        string messageType = msh.GetComponent(9,1);
        if (!SupportedTypes.Contains(messageType))
            return Reject(200, "Unsupported message type", "MSH", 9, $"Message type '{messageType}' is not supported by this interface");

            //Msg content problems; AE- msg read but data has errors
        var pid = msg.GetSegment("PID");
        if (pid is null)
            return Error(100, "Segment sequence error", "PID", null, "PID segment is required");

        if (pid.GetField(3) == "")
            return Error(101, "Required field missing", "PID", 3, "PID-3 (Patient Identifier List) is required");

        return ValidationResult.Accept;   

    }
    private static ValidationResult Reject(int code, string codeText, string segment, int? field, string text) =>
        new("AR", code, codeText, segment, field, text);

    private static ValidationResult Error(int code, string codeText, string segment, int? field, string text) =>
        new("AE", code, codeText, segment, field, text);
}
