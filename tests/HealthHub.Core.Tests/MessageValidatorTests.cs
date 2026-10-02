using HealthHub.Core.Hl7;
using Newtonsoft.Json.Converters;

namespace HealthHub.Core.Tests;

public class MessageValidatorTests
{
    private const string GoodMsh = @"MSH|^~\&|REGADT|MERCYHOSP|HUBENGINE|MERCYHOSP|20261001090000||ADT^A01^ADT_A01|MSG10001|P|2.5.1";

    private const string TrainingMsh = @"MSH|^~\&|LIS|MERCYLAB|HUBENGINE|MERCYHOSP|20261001102000||ORU^R01^ORU_R01|LAB10002|T|2.5.1";

    private const string BillingMsh = @"MSH|^~\&|BILLING|MERCYHOSP|HUBENGINE|MERCYHOSP|20261001093000||DFT^P03^DFT_P03|MSG10003|P|2.5.1";

    private const string GoodPid = @"PID|1||200001^^^MERCYHOSP^MR||GARCIA^MARIA^L";

    private const string PidWithoutId = @"PID|1||||NGUYEN^THANH";

    private static ValidationResult ValidateSegments(params string[] segments) => MessageValidator.Validate(Hl7Message.Parse(string.Join("\r", segments)));

    [Fact]
    public void Good_message()
    {
        var result = ValidateSegments(GoodMsh, GoodPid);

        Assert.Equal("AA", result.AckCode); //should see "AA"

    }
    [Fact]
    public void Missing_patient_identifier()
    {
        var result = ValidateSegments(GoodMsh, PidWithoutId);

        Assert.Equal("AE", result.AckCode); 
        Assert.Equal(101, result.ErrorCode);
        Assert.Equal("PID", result.Segment);
        Assert.Equal(3, result.Field);
    }
    [Fact]
    public void Training_message_sent_to_production()
    {
       var result = ValidateSegments(TrainingMsh, GoodPid);

       Assert.Equal("AR", result.AckCode);
       Assert.Equal(202, result.ErrorCode); 
    }
    [Fact]
    public void Billing_message_on_this_interface()
    {
        var result = ValidateSegments(BillingMsh, GoodPid);

        Assert.Equal("AR", result.AckCode);
        Assert.Equal(200, result.ErrorCode);
    }
    [Fact]
    public void Training_message_that_also_lacks_a_patient_identifier()
    {
        var result = ValidateSegments(TrainingMsh, PidWithoutId);

        Assert.Equal("AR", result.AckCode);
        Assert.Equal(202, result.ErrorCode);
    }
    [Fact]
    public void No_pid_segment_at_all()
    {
        var result = ValidateSegments(GoodMsh);

        Assert.Equal("AE", result.AckCode);
        Assert.Equal(100, result.ErrorCode);
    }
}