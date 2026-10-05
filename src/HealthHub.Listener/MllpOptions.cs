using System.Dynamic;

namespace HealthHub.Listener;

///Seetings from Mllp section of appsettings.json
/// 

public sealed class MllpOptions
{
    public int Port { get; set;} = 6661;
    public string InterfaceName { get; set; } = "";
    public string ProcessingId { get; set; } = "P";
}