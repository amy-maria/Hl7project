using HealthHub.Core.Mllp;

namespace HealthHub.Core.Tests;

public class MllpFrameReaderTests
{
    [Fact]
    public async Task Reads_one_framed_message()
    {
        var stream = new MemoryStream(MllpFraming.Wrap("MSH|^~\\&|A"));
        var reader = new MllpFrameReader(stream);

        Assert.Equal("MSH|^~\\&|A", await reader.ReadMessageAsync());
        Assert.Null(await reader.ReadMessageAsync()); //end of stream, close connection
    }
    [Fact]
    public async Task Reads_two_messages_that_arrive_together()
    {
        byte[] both = [.. MllpFraming.Wrap("FIRST"), .. MllpFraming.Wrap("SECOND")];
        var reader = new MllpFrameReader(new MemoryStream(both));

        Assert.Equal("FIRST", await reader.ReadMessageAsync());
        Assert.Equal("SECOND", await reader.ReadMessageAsync());
    }
    [Fact]
    public async Task Reads_a_message_that_arrives_one_byte_at_a_time()
    {
        var reader = new MllpFrameReader(new OneByteAtATimeStream(MllpFraming.Wrap("SLOW MESSAGE")));

        Assert.Equal("SLOW MESSAGE", await reader.ReadMessageAsync());
    }
    [Fact]
    public async Task Ignores_junk_before_the_start_block()
    {
        byte[] data = [0x41, 0x42, .. MllpFraming.Wrap("REAL")];
        var reader = new MllpFrameReader(new MemoryStream(data));

        Assert.Equal("REAL", await reader.ReadMessageAsync());
    }
    //simulated slow netword, every read returns 1 byte
    private sealed class OneByteAtATimeStream(byte[] data) : MemoryStream(data)
    {
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default) =>
            base.ReadAsync(buffer.Slice(0, Math.Min(1, buffer.Length)), ct);
            
    }
}