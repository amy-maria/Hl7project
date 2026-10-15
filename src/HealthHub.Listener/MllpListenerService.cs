using System.Net;
using System.Net.Sockets;
using HealthHub.Core.Hl7;
using HealthHub.Core.Mllp;
using HealthHub.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace HealthHub.Listener;

public sealed class MllpListenerService : BackgroundService
{
    private readonly MessageRepository _repository;
    private readonly MllpOptions _options;
    private readonly ILogger<MllpListenerService> _logger;

    public MllpListenerService(MessageRepository repository, IOptions<MllpOptions> options, ILogger<MllpListenerService> logger)
    {      
        _repository = repository;
        _options = options.Value;
        _logger = logger;
    }
//runs when the app starts and keeps running until stopped

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int interfaceId = await _repository.GetInterfaceIdAsync(_options.InterfaceName);

        //loopback

        var listener = new TcpListener(IPAddress.Loopback, _options.Port);
        listener.Start();
        _logger.LogInformation("Listening for '{Interface}' on port {Port}", _options.InterfaceName, _options.Port);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                TcpClient client = await listener.AcceptTcpClientAsync(stoppingToken);
                //tells it not to wait, acept the next sender
                _ = HandleConnectionAsync(client, interfaceId, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            //app is shutting down
        }
        finally
        {
            listener.Stop();
        }
    }
    private async Task HandleConnectionAsync(TcpClient client, int interfaceId, CancellationToken ct)
    {
        var remote = client.Client.RemoteEndPoint;
        _logger.LogInformation("Connection opened from {Remote}", remote);

        try
        {
            using (client)
            {
                NetworkStream stream = client.GetStream();
                var reader = new MllpFrameReader(stream);

                while (true)
                {
                    string? raw = await reader.ReadMessageAsync(ct);
                    if (raw is null)
                        break; //sender closed the connection

                    string? ack = await ProcessMessageAsync(raw, interfaceId);
                    if (ack is null)
                        break; //could not process msg, close w/o ACK

                    await stream.WriteAsync(MllpFraming.Wrap(ack), ct);
                }
            }
        }
        catch (OperationCanceledException)
        {
            //shutting down
        }
        catch (Exception ex)
        {
            //db down, no ACK is sent
            _logger.LogError(ex, "Error on connection from {Remote}; no ACK sent", remote);
        }
        _logger.LogInformation("Connection close from {Remote}", remote);
    }
    ///Validates and stores one message, then return the ACK to send for null for none.
    /// 
    private async Task<string?> ProcessMessageAsync(string raw, int interfaceId)
    {
        Hl7Message message;
        try
        {
            message = Hl7Message.Parse(raw);
        }
        catch (FormatException ex)
        {
            _logger.LogError("Unparseable message; no ACK sent: {Error}", ex.Message);
            return null;
        }

        //-- Clinical Validation Step --
        // Initalize the Engine

        var validator = new MessageValidator();

        validator.AddRule(new RequiredSegmentRule("MSH"));
        validator.AddRule(new RequiredSegmentRule("EVN"));
        validator.AddRule(new RequiredSegmentRule("PID"));
        validator.AddRule(new RequiredSegmentRule("PV1"));
        
        //Uuse NotEmptyRule for fields that must have data
        validator.AddRule(new NotEmptyRule("MSH", 10));
        validator.AddRule(new NotEmptyRule("PID", 8));

        //Use RegEx field rule for fields that must follow a pattern
        validator.AddRule(new RegexFieldRule("PID", 3, @"^[a-zA-Z0-9]{5,10}"));
        validator.AddRule(new RegexFieldRule("PID", 8, @"^[MFU]$")); //sex must be M,F,U

        //Run the validation
        ValidationResult result = validator.Validate(message);

        //Endof validation

        var record = MessageRecord.FromHl7(message, raw, interfaceId);
        record.Status = result.IsAccepted ? "Processed" : "Error";
        record.AckCode = result.AckCode;
        record.ErrorText = result.ErrorText;

        //store before acknowledging; never ACK a message that was not savely saved
        long messageId = await _repository.InsertAsync(record);
        _logger.LogInformation("Message {Id}: {Type} control ID {ControlId} -> {Ack} {Error}", messageId, message.MessageType, message.ControlId, result.AckCode, result.ErrorText);

        return AckBuilder.Build(message, result, NewAckControlId(), DateTimeOffset.Now);
    }
    //"ACK" - 17 digits = 20 char, MSH-10 limit in v 2.5.1
    private static string NewAckControlId() => "ACK" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
    
    }