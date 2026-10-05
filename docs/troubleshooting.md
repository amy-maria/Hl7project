
Cause: The listener is down

Observed: 
   Connection refused
   at System.Net.Sockets.Socket.AwaitableSocketAsyncEventArgs.ThrowException(SocketError error, CancellationToken cancellationToken)
   at System.Net.Sockets.Socket.AwaitableSocketAsyncEventArgs.System.Threading.Tasks.Sources.IValueTaskSource.GetResult(Int16 token)
   at System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c.<.cctor>b__4_0(Object state)
--- End of stack trace from previous location ---
   at System.Net.Sockets.TcpClient.CompleteConnectAsync(Task task)
   at Program.<<Main>$>g__SendAsync|0_4(String[] a) in /Users/ar/hl7project/HealthHub/src/HealthHub.Tools/Program.cs:line 95
   at Program.<Main>$(String[] args) in /Users/ar/hl7project/HealthHub/src/HealthHub.Tools/Program.cs:line 21
   at Program.<Main>(String[] args)

Remedy: 
No message was sent so the data can't be lost or duplicated. The message is still with the sender. This is what it looks like when the interface engine is down.

_________________________________________________________________________________________
Cause: The db is down

Observed:

Listener Messages- 
info: HealthHub.Listener.MllpListenerService[0]
      Connection opened from 127.0.0.1:49310
fail: HealthHub.Listener.MllpListenerService[0]
      Error on connection from 127.0.0.1:49310; no ACK sent
      Microsoft.Data.SqlClient.SqlException (0x80131904): A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: TCP Provider, error: 35 - An internal exception was caught)
       ---> System.Net.Sockets.SocketException (61): Connection refused ...
--- End of stack trace from previous location ---
         at Dapper.SqlMapper.ExecuteScalarImplAsync[T](IDbConnection cnn, CommandDefinition command) in /_/Dapper/SqlMapper.Async.cs:line 1241
         at HealthHub.Data.MessageRepository.InsertAsync(MessageRecord message) in /Users/ar/hl7project/HealthHub/src/HealthHub.Data/MessageRepository.cs:line 40
         at HealthHub.Data.MessageRepository.InsertAsync(MessageRecord message) in /Users/ar/hl7project/HealthHub/src/HealthHub.Data/MessageRepository.cs:line 40
         at HealthHub.Listener.MllpListenerService.ProcessMessageAsync(String raw, Int32 interfaceId) in /Users/ar/hl7project/HealthHub/src/HealthHub.Listener/MllpListenerService.cs:line 115
         at HealthHub.Listener.MllpListenerService.HandleConnectionAsync(TcpClient client, Int32 interfaceId, CancellationToken ct) in /Users/ar/hl7project/HealthHub/src/HealthHub.Listener/MllpListenerService.cs:line 70
      ClientConnectionId:00000000-0000-0000-0000-000000000000
      Error Number:10061,State:0,Class:20
info: HealthHub.Listener.MllpListenerService[0]
      Connection close from 127.0.0.1:49310

Sender Messages -
Sent adt_a01_admit.hl7 to port 6661. Waiting for ACK ...
No ACK within 10 sec (timeout).

Remedy: After re-starting the server, I resent the same ADT message.

Listener: 
info: HealthHub.Listener.MllpListenerService[0]
      Message 20001: ADT^A01^ADT_A01 control ID MSG10001 -> AA (null)
info: HealthHub.Listener.MllpListenerService[0]
      Connection close from 127.0.0.1:49371

Sender: Sent adt_a01_admit.hl7 to port 6661. Waiting for ACK ...
ACK received: 
MSH|^~\&|HUBENGINE|MERCYHOSP|REGADT|MERCYHOSP|20261005173819-0400||ACK^A01^ACK|ACK20261005213819933|P|2.5.1
MSA|AA|MSG10001.  <- This is the first retry after the restart of the db

Cause: The poison message
Msg sent: HELLO, THIS IS NOT AN HL7 MESSAGE

Observed:
Sender:
Sent not_hl7.hl7 to port 6661. Waiting for ACK ...

Listener: 
Connection opened from 127.0.0.1:49389
fail: HealthHub.Listener.MllpListenerService[0]
      Unparseable message; no ACK sent: Message must begin with an MSH segment.
info: HealthHub.Listener.MllpListenerService[0]
      Connection close from 127.0.0.1:49389

Sender: The receiver closed the connection without sending ACK.

Remedy: One malformed message can block an entire feed: no admissions, no results. If you continually resent the msg, it will back up the interface. Instead, the msg needs to be "deleted" or moved to an error queue (best practice).