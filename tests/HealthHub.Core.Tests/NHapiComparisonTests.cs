using System.IO.Pipes;
using HealthHub.Core.Hl7;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Newtonsoft.Json.Bson;
using NHapi.Base.Parser;
using NHapi.Base.Util;
using NHapi.Model.V251.Message;

namespace HealthHub.Core.Tests;

///checks hand built parser against NHapi library

public class NHapiComparionTests
{
    private static readonly string SampleAdt = string.Join("\r", @"MSH|^~\&|REGADT|MERCYHOSP|HUBENGINE|MERCYHOSP|20260929083015||ADT^A01^ADT_A01|MSG00001|P|2.5.1",
        @"EVN|A01|20260929083000",
        @"PID|1||123456^^^MERCYHOSP^MR~E998877^^^MERCYEMPI^PI||DOE^JANE^A||19800115|F",
        @"PV1|1|I|4WEST^401^A^MERCYHOSP||||1234^SMITH^JOHN^^^^MD");

    [Fact]
    public void Both_parsers_agree_on_key_field()
    {
        var mine = Hl7Message.Parse(SampleAdt);
        var nhapi = (ADT_A01)new PipeParser().Parse(SampleAdt);

        Assert.Equal(nhapi.MSH.MessageControlID.Value, mine.ControlId);
        Assert.Equal(nhapi.PID.GetPatientName(0).FamilyName.Surname.Value, mine.GetSegment("PID")!.GetComponent(5, 1));
        
        var terser = new Terser(nhapi);
        Assert.Equal("E998877", terser.Get("/PID-3(1)-1"));
        Assert.Equal("401", terser.Get("/PV1-3-2"));
        Assert.Equal("SMITH", nhapi.PV1.GetAttendingDoctor(0).FamilyName.Surname.Value);
        Assert.Equal("SMITH", terser.Get("/PV1-7-2"));
        Assert.Equal("SMITH", mine.GetSegment("PV1")!.GetComponent(7, 2));

    }
}