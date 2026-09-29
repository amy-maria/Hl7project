using System.Configuration.Assemblies;
using HealthHub.Core.Hl7;

  namespace HealthHub.Core.Tests;

  public class Hl7MessageTests
  {
    private static readonly string SampleAdt = string.Join("\r", 
    @"MSH|^~\&|REGADT|MERCYHOSP|HUBENGINE|MERCYHOSP|20260929083015||ADT^A01^ADT_A01|MSG00001|P|2.5.1",
        @"EVN|A01|20260929083000",
        @"PID|1||123456^^^MERCYHOSP^MR~E998877^^^MERCYEMPI^PI||DOE^JANE^A||19800115|F|||123 MAIN ST^^SPRINGFIELD^IL^62701||(217)555-0100",
        @"PV1|1|I|4WEST^401^A^MERCYHOSP||||1234^SMITH^JOHN^^^^MD");

        [Fact]
        public void Reads_message_type_and_controls_id()
        {
            var msg = Hl7Message.Parse(SampleAdt);
            
            Assert.Equal("ADT^A01^ADT_A01", msg.MessageType);
            Assert.Equal("MSG00001", msg.ControlId);
        }
        [Fact]
        public void Msh_field_numbering_matches_spec()
        {
            var msh = Hl7Message.Parse(SampleAdt).GetSegment("MSH")
            !;

            Assert.Equal("|", msh.GetField(1));
            Assert.Equal(@"^~\&", msh.GetField(2));
            Assert.Equal("REGADT", msh.GetField(3));
        }
        [Fact]
        public void Reads_patient_name_components()
        {
            var pid = Hl7Message.Parse(SampleAdt).GetSegment("PID")!;

            Assert.Equal("DOE", pid.GetComponent(5, 1));
            Assert.Equal("JANE", pid.GetComponent(5, 2));
        }

        [Fact]
        public void Rejects_message_without_msh()
        {
            Assert.Throws<FormatException>(() => Hl7Message.Parse("PID|1||123456"));

        }

        [Fact]
        public void Pid3_has_two_repetitions()
    {
        var msg = Hl7Message.Parse(SampleAdt);
        var pid = msg.GetSegment("PID")!;
        var ids = pid.GetRepetitions(3);

        Assert.Equal(2, ids.Length);
        //Each repetition is a CX data type
        var secondId = ids[1].Split(msg.Delimiters.Component);
        Assert.Equal("E998877", secondId[0]);
        Assert.Equal("PI", secondId[4]);
    }
    [Fact]
    public void Empty_field_has_no_repetitions()
    {
        var pid = Hl7Message.Parse(SampleAdt).GetSegment("PID")!;

        Assert.Empty(pid.GetRepetitions(2));
    }

  }