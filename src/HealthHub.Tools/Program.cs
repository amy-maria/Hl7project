using HealthHub.Core.Hl7;
using HealthHub.Data;
using Microsoft.Extensions.Configuration;
using System.Net.Sockets;
using HealthHub.Core.Mllp;
using HealthHub.Transforms; 
//settings from user secrets (local only) and env variables

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .Build();

if (args.Length == 0)
    return PrintUsage();

switch (args[0])
{
    case "import": return await ImportAsync(args);
    case "recent": return await RecentAsync();
    case "send": return await SendAsync(args);
    case "transform": return await TransformAsync(args);
    default:       return PrintUsage();

}

//import <folder> "<interface name>": parse every HL7 file in a folder and save it

async Task<int> ImportAsync(string[] a)
{
    if (a.Length < 3)
        return PrintUsage();

    string folder = a[1];
    string interfaceName = a[2];

    var repository = new MessageRepository(GetConnectionString());
    int interfaceId = await repository.GetInterfaceIdAsync(interfaceName);

    foreach (string path in Directory.GetFiles(folder, "*.hl7"))
    {
        string fileName = Path.GetFileName(path);
        string raw = await File.ReadAllTextAsync(path);

        try
        {
            var message = Hl7Message.Parse(raw);
            var record = MessageRecord.FromHl7(message, raw, interfaceId);
            long id = await repository.InsertAsync(record);

            Console.WriteLine($"{fileName} -> MessageId {id} ({record.MessageType}^{record.TriggerEvent}, control ID {record.ControlId})");
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"{fileName} -> skipped: {ex.Message}");
        }
    }
    return 0;
}
/// recent: show the 10 newest messages in the db

async Task<int> RecentAsync()
    {
        var repository = new MessageRepository(GetConnectionString());

        foreach (var m in await repository.GetRecentAsync(10))
        {
            Console.WriteLine($"{m.MessageId, 5} {m.ReceivedAtUtc:yyyy-MM-dd HH:mm:ss} {m.MessageType}^{m.TriggerEvent, -4} " + $"{m.ControlId, -10} MRN {m.PatientMrn ?? "-",-8} {m.Status, -10}{m.AckCode ?? ""}");
        }
        return 0;
    }
async Task<int> SendAsync(string[] a)
{
    if (a.Length < 2) return PrintUsage();
    string path = a[1];
    int port = a.Length >= 3 ? int.Parse(a[2]) : 6661;

    string message = (await File.ReadAllTextAsync(path)).Replace("\r\n", "\r").Replace('\n', '\r').TrimEnd('\r');
    using var client = new TcpClient();
    await client.ConnectAsync("127.0.0.1", port);
    NetworkStream stream = client.GetStream();

    await stream.WriteAsync(MllpFraming.Wrap(message));
    Console.WriteLine($"Send {Path.GetFileName(path)} to port {port}. Waiting for ACK ...");

    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
    try
    {
        string? ack = await new MllpFrameReader(stream).ReadMessageAsync(timeout.Token);
        if (ack is null)
        {
            Console.WriteLine("The receiver closed the connection without sending ACK.");
            return 2;
        }
        Console.WriteLine("ACK received: ");
        Console.WriteLine(ack.Replace('\r', '\n'));
        return 0;
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("No ACK within 10 sec (timeout).");
        return 2;
    }
}
    
/// <summary>
/// NEW COMMAND: Takes an HL7 file and prints the FHIR JSON version.
/// </summary>
async Task<int> TransformAsync(string[] a)
{
    if (a.Length < 2) return PrintUsage();

    string path = a[1];
    if (!File.Exists(path))
    {
        Console.WriteLine($"Error: File not found at {path} ");
        return 1;
    }
    string raw = await File.ReadAllTextAsync(path);

    try
    {
        //Parse the HL7
        var message = Hl7Message.Parse(raw);
        //Use the transformer to convert to FHIR
        var transformer = new PatientFhirTransformer();
        string json = transformer.Transform(message);
        //output the result
        Console.WriteLine("---FHIR PATIENT RESOURCE ---");
        Console.WriteLine(json);
        return 0;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Transformation failed: {ex.Message}");
        return 1;
    }
}
string GetConnectionString() => config.GetConnectionString("HealthHub") ?? throw new InvalidOperationException("Connection string 'HealthHub' is not set.");

int PrintUsage()
{
    Console.WriteLine("""
    Usage:
        dotnet run -- import <folder> "<interface name>"
        dotnet run -- recent
        dotnet run -- send <file.hl7> [port]
        dotnet run -- transform <file.hl7>
    """);
    return 1;
}