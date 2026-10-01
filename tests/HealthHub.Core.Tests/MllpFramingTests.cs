using HealthHub.Core.Mllp;

namespace HealthHub.Core.Tests;

public class MllpFamingTests
{
    [Fact]
    public void Wrap_adds_start_and_end_bytes()
    {
        byte[] framed = MllpFraming.Wrap("MSH|A");

        Assert.Equal((byte)0x0B, framed[0]);
        Assert.Equal((byte)0x1C, framed[framed.Length -2]);
        Assert.Equal((byte)0x0D, framed[framed.Length -1]);
    }
    [Theory]
    [InlineData("MSH|A", 8)]
    [InlineData("NGUYỄN", 11)]
    public void Wrap_length_is_message_bytes_plus_three(string message, int expectedLength)
    {
        Assert.Equal(expectedLength, MllpFraming.Wrap(message).Length);
    }

}