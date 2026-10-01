using System.Configuration.Assemblies;
using HealthHub.Core.Hl7;
using HealthHub.Data;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

namespace HealthHub.Data.Tests;

public class MessageRecordTests
{
    //MR is on purpose the second repetition in PID-3
    private static readonly string SampleAdt = string.Join("\r", 
    @"MSH|^~\&|REGADT|MERCYHOSP|HUBENGINE|MERCYHOSP|20261001090000||ADT^A01^ADT_A01|MSG10001|P|2.5.1",
        @"EVN|A01|20261001090000",
        @"PID|1||E200001^^^MERCYEMPI^PI~200001^^^MERCYHOSP^MR||GARCIA^MARIA^L||19720614|F",
        @"PV1|1|I|3EAST^310^B^MERCYHOSP");

    [Fact]
    public void Copies_header_fields_from_msh()
    {
        var record = MessageRecord.FromHl7(Hl7Message.Parse(SampleAdt), SampleAdt, interfaceId: 1);

        Assert.Equal(1, record.InterfaceId);
        Assert.Equal("REGADT", record.SendingApplication);
        Assert.Equal("MERCYHOSP", record.SendingFacility);
        Assert.Equal("HUBENGINE", record.ReceivingApplication);
        Assert.Equal(new DateTime(2026, 10, 1, 9, 0,0), record.MessageDateTime);
        Assert.Equal("ADT", record.MessageType);
        Assert.Equal("A01", record.TriggerEvent);
        Assert.Equal("ADT_A01", record.MessageStructure);
        Assert.Equal("MSG10001", record.ControlId);
        Assert.Equal("P", record.ProcessingId);
        Assert.Equal("2.5.1", record.VersionId);
        Assert.Equal(SampleAdt, record.RawMessage);
        Assert.Equal("Received", record.Status);
    }
    [Fact]
    public void Finds_mrn_by_identifier_type_not_position()
    {
        var record = MessageRecord.FromHl7(Hl7Message.Parse(SampleAdt), SampleAdt, interfaceId: 1);
            
            Assert.Equal("200001", record.PatientMrn);
    }
    [Fact]
    public void Mrn_is_null_when_no_mr_identifier()
    {
        string noMr = SampleAdt.Replace("~200001^^^MERCYHOSP^MR", "");
        
        var record = MessageRecord.FromHl7(Hl7Message.Parse(noMr), noMr, interfaceId: 1);

        Assert.Null(record.PatientMrn);
    }
}