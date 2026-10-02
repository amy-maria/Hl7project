using HealthHub.Core.Hl7;
using Microsoft.VisualStudio.TestPlatform.Common.Utilities;

namespace HealthHub.Core.Tests;

public class AckBuilderTests
{
    private static readonly Hl7Message LabResult = Hl7Message.Parse(string.Join("\r", @"MSH|^~\&|LIS|MERCYLAB|HUBENGINE|MERCYHOSP|20261001101500||ORU^R01^ORU_R01|LAB10001|P|2.5.1",
        @"PID|1||200001^^^MERCYHOSP^MR||GARCIA^MARIA^L"));

    private static readonly DateTimeOffset FixedTime = new(2026, 10, 2, 14, 30, 5, TimeSpan.FromHours(-4));

    [Fact]
    public void Accepted_msg_gets_msh_and_msa()
    {
        string ack = AckBuilder.Build(LabResult, ValidationResult.Accept, "ACK001", FixedTime);
        string[] segments = ack.Split('\r');
        
        Assert.Equal(2, segments.Length);
        Assert.Equal(@"MSH|^~\&|HUBENGINE|MERCYHOSP|LIS|MERCYLAB|20261002143005-0400||ACK^R01^ACK|ACK001|P|2.5.1", segments[0]);
        Assert.Equal(@"MSA|AA|LAB10001", segments[1]);
       

    }
    [Fact]
    public void Error_text_is_escaped()
    {
        var result = new ValidationResult("AE", 207, "Application internal error", ErrorText: "Lab & Path | down");
        string ack = AckBuilder.Build(LabResult, result, "ACK003", FixedTime);

        Assert.EndsWith(@"Lab \T\ Path \F\ down", ack);
    }
        [Fact]
    public void Rejected_message_also_gets_an_err_segment()
    {
        var result = new ValidationResult("AE", 101, "Required field missing", "PID", 3, "PID-3 is required");

        string ack = AckBuilder.Build(LabResult, result, "ACK002", FixedTime);

        string[] segments = ack.Split('\r');
        Assert.Equal(3, segments.Length);    // predict: how many segments?
        Assert.Equal(@"MSA|AE|LAB10001", segments[1]);    // predict: the MSA segment
        Assert.Equal("ERR||PID^1^3|101^Required field missing^HL70357|E||||PID-3 is required", segments[2]);
    }
    
    }
