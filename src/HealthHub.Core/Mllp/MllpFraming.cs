using System.Text;

namespace HealthHub.Core.Mllp;

public static class MllpFraming
{
    public const byte StartBlock = 0x0B; //<VT> vertical tab, msg start
    public const byte EndBlock = 0x1C; //<FS> file separator, msg ended
    public const byte CarriageReturn = 0x0D; //<CR> always follows <FS>

    ///wraps a message as &lt;VT&gt;message&lt;FS&gt;&lt&CR&gt;, ready to send
    
    public static byte[] Wrap(string message)
    {
        byte[] body = Encoding.UTF8.GetBytes(message);
        byte[] framed = new byte[body.Length + 3];

        framed[0] = StartBlock;
        body.CopyTo(framed, 1);
        framed[framed.Length -2] = EndBlock;
        framed[framed.Length -1] = CarriageReturn;

        return framed;
    }
}