using System.Text;

namespace HealthHub.Core.Mllp;

///reads MLLP-framed msgs from a stream. TCP delivers stream of bytes. This class collects all the bytes until it has a complete frame.
///
public sealed class MllpFrameReader
{
    private readonly Stream _stream;
    private readonly byte[] _readBuffer = new byte[8192];
    private readonly List<byte> _pending = new (); //bytes received but not yet returned

    public MllpFrameReader(Stream stream)
    {
        _stream = stream;
    }

    ///Returns the next message or null when the other side closes the connection.
    
    public async Task<string?> ReadMessageAsync(CancellationToken ct = default)
    {
        while (true)
        {
            string? message = TryTakeFrame();
            if (message is not null)
                return message;
            int bytesRead = await _stream.ReadAsync(_readBuffer, ct);
            if (bytesRead == 0 )
                return null; //connection closed

            _pending.AddRange(_readBuffer.Take(bytesRead));
        }
    }
    //Looks for <VT> ...<FS><CR> in pending bytes. Returns the msg inside or null if incomplete.
    private string? TryTakeFrame()
    {
        int start = _pending.IndexOf(MllpFraming.StartBlock);
        if (start <0)
        {
            _pending.Clear(); //no start block, no useful info
            return null;
        }

        for (int i = start + 1; i < _pending.Count -1; i++)
        {
            if (_pending[i] == MllpFraming.EndBlock && _pending[i + 1] == MllpFraming.CarriageReturn)
            {
                byte[] body = _pending.GetRange(start + 1, i - start -1).ToArray();
                _pending.RemoveRange(0, i+2); //remove this frame and any junk before it
                return Encoding.UTF8.GetString(body);
            }
            }
            if (start > 0)
                _pending.RemoveRange(0, start); //drop junk before the start block and keep waiting
                return null;
    
    }
}