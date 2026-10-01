using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Dapper;
using Microsoft.Data.SqlClient;
///bring in new packages, SqlConnect from SqlClient and async from Dapper
/// 
namespace HealthHub.Data;

///all Sql for the message store
public sealed class MessageRepository
{
    private readonly string _connectionString;

    public MessageRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<int> GetInterfaceIdAsync(string interfaceName)
    {
        const string sql = "SELECT InterfaceId FROM dbo.Interfaces WHERE Name = @Name;";

        await using var connection = new SqlConnection(_connectionString);
        int? id = await connection.QuerySingleOrDefaultAsync<int?>(sql, new{Name = interfaceName});

        return id ?? throw new InvalidOperationException($"No interface name '{interfaceName}' in dob.Interfaces. Did you run 003_seed_data.sql?");

    }
    ///saves a message and returns a new messagId.
    
    public async Task<long> InsertAsynch(MessageRecord message)
    {
        const string sql = """
            INSERT INTO dbo.Messages (InterfaceId, SendingApplication, SendingFacility, ReceivingApplication, ReceivingFacility, MessageDateTime, MessageType, TriggerEvent, MessageStructure, ControlId, ProcessingId, VersionId, PatientMrn, RawMessage, Status, AckCode, ErrorText) 
            OUTPUT INSERTED.MessageId
            VALUES (@InterfaceId, @SendingApplication, @SendingFacility, @ReceivingApplication, @ReceivingFacility, @MessageDateTime, @MessageType, @TriggerEvent, @MessageStructure, @ControlId, @ProcessingId, @VersionId, @PatientMrn, @RawMessage, @Status, @AckCode, @ErrorText);
            """;

            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<long>(sql, message);
    }

    ///Returns the newest messages, newest first
    public async Task<IReadOnlyList<MessageRecord>> GetRecentAsync(int count)
    {
        const string sql = """
            SELECT TOP (@Count) * 
            FROM dbo.Messages
            ORDER BY ReceivedAtUtc DESC;
        """;

        await using var connection = new SqlConnection(_connectionString);
        var rows = await connection.QueryAsync<MessageRecord>(sql, new { Count = count});
        return rows.ToList();
    }
}