# 🛠️ HealthHub Troubleshooting Guide (Runbook)

This document provides guidance for technical staff to identify, diagnose, and remediate common failures within the HealthHub Interoperability Engine.

---

## 🚨 Scenario 1: Listener Service Unavailable
**Technical Cause:** The MLLP Listener service is not running or the port (6661) is blocked by a firewall.

### Observed Symptoms
**Sender Side (Tools):**
`Connection refused at System.Net.Sockets.TcpClient.ConnectAsync.`

**Listener Side:**
No logs are generated because the service is not active to receive the connection.

### Clinical Impact
**Low-to-Medium Risk.** No data is lost. The data remains with the sending system (e.g., the Registration System). However, there is a delay in patient care because the clinical staff will not see new admissions in the EMR until the service is restored.

### Remedy
1. Verify the `HealthHub.Listener` service is running in the Windows Services manager or via Docker.
2. Ensure port `6661` is open on the local firewall.
3. Restart the service if it has hung.

---

## 🚨 Scenario 2: Database Persistence Failure
**Technical Cause:** The SQL Server instance is offline, unreachable, or the connection string is invalid.

### Observed Symptoms
**Listener Side (Error Log):**
`fail: HealthHub.Listener.MllpListenerService[0]`
`Microsoft.Data.SqlClient.SqlException: A network-related or instance-specific error occurred while establishing a connection to SQL Server.`

**Sender Side (Tools):**
`No ACK within 10 sec (timeout).`

### Clinical Impact
**High Risk.** This is a critical failure. The Listener accepts the TCP connection and the message, but because it cannot write the record to the database, the message is lost during the transaction failure. This can result in missing patient records in the system.

### Remedy
1. Verify the SQL Server service is running in Docker/Windows.
2. Validate the connection string in `appsettings.json` or User Secrets.
3. Once the database is back online, the sender must manually resend the lost messages.

---

## 🚨 Scenario 3: The "Poison Message" (Protocol Violation)
**Technical Cause:** A sender is transmitting data that does not follow the HL7 v2 standard (e.g., a text file sent to an MLLP port).

### Observed Symptoms
**Listener Side (Error Log):**
`fail: HealthHub.Listener.MllpListenerService[0]`
`Unparseable message; no ACK sent: Message must begin with an MSH segment.`

**Sender Side (Tools):**
`The receiver closed the connection without sending ACK.`

### Clinical Impact
**High Risk (Pipeline Blockage).** A single "poison message" can act as a bottleneck. If the sender's system is automated, it may attempt to resend the bad message repeatedly, effectively blocking the entire interface feed and preventing valid patient messages from being processed.

### Remedy
1. Identify the source of the malformed message using the `MSH-3` (Sending Application) field from the logs.
2. Contact the sending system's technical team to correct the data format.
3. **Best Practice:** Implement an Error Queue (Dead Letter Queue) to move poison messages out of the main pipeline so valid traffic can continue.

---

## 🔍 Quick Reference Table

| Error Type | Error Code | Primary Symptom | Clinical Impact | Severity |
| :--- | :--- | :--- | :--- | :--- |
| **Listener Down** | `SocketException` | Connection Refused | Delay in care | Medium |
| **DB Down** | `SqlException` | Timeout / No ACK | Potential Data Loss | **CRITICAL** |
| **Poison Msg** | `FormatException` | Connection Closed | Interface Blockage | **HIGH** |
