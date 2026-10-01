USE HealthHub;
GO

DELETE FROM dbo.Messages;
DELETE FROM dbo.Interfaces;
DBCC CHECKIDENT ('dbo.Messages', RESEED, 0);
DBCC CHECKIDENT ('dbo.Interfaces', RESEED, 0);
GO

INSERT INTO dbo.Interfaces (Name, Direction, RemoteHost, Port)
VALUES (N'ADT from Registration', 'IN',  N'reg.mercyhosp.local', 6661),
       (N'Results from LIS',      'IN',  N'lis.mercyhosp.local', 6662),
       (N'Orders to LIS',         'OUT', N'lis.mercyhosp.local', 6663);
GO

DECLARE @adt INT = (SELECT InterfaceId FROM dbo.Interfaces WHERE Name = N'ADT from Registration');
DECLARE @lis INT = (SELECT InterfaceId FROM dbo.Interfaces WHERE Name = N'Results from LIS');
DECLARE @cr  NCHAR(1) = NCHAR(13);   -- HL7 segment terminator: carriage return
DECLARE @now DATETIME2(3) = SYSUTCDATETIME();

INSERT INTO dbo.Messages
    (InterfaceId, SendingApplication, MessageType, TriggerEvent, ControlId, ProcessingId,
     PatientMrn, RawMessage, ReceivedAtUtc, Status, AckCode, ErrorText)
VALUES
-- 1. Admit Jane Doe
(@adt, N'REGADT', 'ADT', 'A01', N'MSG00001', 'P', N'123456',
 CONCAT(N'MSH|^~\&|REGADT|MERCYHOSP|HUBENGINE|MERCYHOSP|20260930080000||ADT^A01^ADT_A01|MSG00001|P|2.5.1', @cr,
        N'PID|1||123456^^^MERCYHOSP^MR||DOE^JANE^A||19800115|F', @cr,
        N'PV1|1|I|4WEST^401^A^MERCYHOSP'),
 DATEADD(MINUTE, -240, @now), 'Processed', 'AA', NULL),

-- 2. Update Jane's phone number
(@adt, N'REGADT', 'ADT', 'A08', N'MSG00002', 'P', N'123456',
 CONCAT(N'MSH|^~\&|REGADT|MERCYHOSP|HUBENGINE|MERCYHOSP|20260930083000||ADT^A08^ADT_A01|MSG00002|P|2.5.1', @cr,
        N'PID|1||123456^^^MERCYHOSP^MR||DOE^JANE^A||19800115|F|||||(217)555-0199', @cr,
        N'PV1|1|I|4WEST^401^A^MERCYHOSP'),
 DATEADD(MINUTE, -200, @now), 'Processed', 'AA', NULL),

-- 3. Admit with no patient identifier
(@adt, N'REGADT', 'ADT', 'A01', N'MSG00003', 'P', NULL,
 CONCAT(N'MSH|^~\&|REGADT|MERCYHOSP|HUBENGINE|MERCYHOSP|20260930091000||ADT^A01^ADT_A01|MSG00003|P|2.5.1', @cr,
        N'PID|1||||SMITH^ROBERT||19550302|M', @cr,
        N'PV1|1|E|ED^12^^MERCYHOSP'),
 DATEADD(MINUTE, -150, @now), 'Error', 'AE', N'PID-3 (Patient Identifier List) is required'),

-- 4. Potassium result for Jane
(@lis, N'LIS', 'ORU', 'R01', N'LAB0001', 'P', N'123456',
 CONCAT(N'MSH|^~\&|LIS|MERCYLAB|HUBENGINE|MERCYHOSP|20260930091500||ORU^R01^ORU_R01|LAB0001|P|2.5.1', @cr,
        N'PID|1||123456^^^MERCYHOSP^MR||DOE^JANE^A', @cr,
        N'OBR|1|ORD1001|ACC26-0001|2823-3^Potassium^LN', @cr,
        N'OBX|1|NM|2823-3^Potassium^LN||5.9|mmol/L|3.5-5.1|H|||F'),
 DATEADD(MINUTE, -90, @now), 'Processed', 'AA', NULL),

-- 5. The same potassium result again
(@lis, N'LIS', 'ORU', 'R01', N'LAB0001', 'P', N'123456',
 CONCAT(N'MSH|^~\&|LIS|MERCYLAB|HUBENGINE|MERCYHOSP|20260930091500||ORU^R01^ORU_R01|LAB0001|P|2.5.1', @cr,
        N'PID|1||123456^^^MERCYHOSP^MR||DOE^JANE^A', @cr,
        N'OBR|1|ORD1001|ACC26-0001|2823-3^Potassium^LN', @cr,
        N'OBX|1|NM|2823-3^Potassium^LN||5.9|mmol/L|3.5-5.1|H|||F'),
 DATEADD(MINUTE, -60, @now), 'Processed', 'AA', NULL),

-- 6. Glucose result sent with processing ID T
(@lis, N'LIS', 'ORU', 'R01', N'LAB0002', 'T', N'123456',
 CONCAT(N'MSH|^~\&|LIS|MERCYLAB|HUBENGINE|MERCYHOSP|20260930094500||ORU^R01^ORU_R01|LAB0002|T|2.5.1', @cr,
        N'PID|1||123456^^^MERCYHOSP^MR||DOE^JANE^A', @cr,
        N'OBR|1|ORD1002|ACC26-0002|2345-7^Glucose^LN', @cr,
        N'OBX|1|NM|2345-7^Glucose^LN||98|mg/dL|70-99|N|||F'),
 DATEADD(MINUTE, -30, @now), 'Error', 'AR', N'MSH-11 processing ID ''T'' not accepted by production receiver');
GO