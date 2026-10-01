USE HealthHub;
GO

DROP TABLE IF EXISTS dbo.Messages;
DROP TABLE IF EXISTS dbo.Interfaces;
GO

CREATE TABLE dbo.Interfaces 
(
    InterfaceId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Interfaces PRIMARY KEY,
    Name        NVARCHAR(100)     NOT NULL CONSTRAINT UQ_Interfaces_Name UNIQUE,
    Direction   VARCHAR(3)        NOT NULL CONSTRAINT CK_Interfaces_Direction CHECK (Direction IN ('IN', 'OUT')),
    RemoteHost  NVARCHAR(255)     NULL,
    Port        INT               NULL,
    IsActive    BIT               NOT NULL CONSTRAINT DF_Interfaces_IsActive DEFAULT(1)

);
GO

CREATE TABLE dbo.Messages
(
    MessageId   BIGINT  IDENTITY(1,1) NOT NULL CONSTRAINT PK_Messages PRIMARY KEY,
    InterfaceId INT                   NOT NULL CONSTRAINT FK_Messages_Interfaces REFERENCES dbo.Interfaces(InterfaceId),
    -- Copied out of the MSH segment to earch and index them
    SendingApplication   NVARCHAR(50)  NULL,     -- MSH-3
    SendingFacility      NVARCHAR(50)  NULL,     -- MSH-4
    ReceivingApplication NVARCHAR(50)  NULL,     -- MSH-5
    ReceivingFacility    NVARCHAR(50)  NULL,     -- MSH-6
    MessageDateTime      DATETIME2(0)  NULL,     -- MSH-7, as the sender wrote it
    MessageType          VARCHAR(3)    NOT NULL, -- MSH-9.1  e.g. ADT
    TriggerEvent         VARCHAR(3)    NULL,     -- MSH-9.2  e.g. A01
    MessageStructure     VARCHAR(7)    NULL,     -- MSH-9.3  e.g. ADT_A01
    ControlId            NVARCHAR(50)  NOT NULL, -- MSH-10
    ProcessingId         CHAR(1)       NULL,     -- MSH-11   P=Production, T=Training, D=Debugging
    VersionId            VARCHAR(10)   NULL,     -- MSH-12   e.g. 2.5.1

    PatientMrn           NVARCHAR(50)  NULL,     -- from PID-3 (the MR repetition)

    RawMessage           NVARCHAR(MAX) NOT NULL, -- exactly what arrived
    ReceivedAtUtc        DATETIME2(3)  NOT NULL
                         CONSTRAINT DF_Messages_ReceivedAtUtc DEFAULT (SYSUTCDATETIME()),

    Status               VARCHAR(20)   NOT NULL
                         CONSTRAINT DF_Messages_Status DEFAULT ('Received')
                         CONSTRAINT CK_Messages_Status CHECK (Status IN ('Received', 'Processed', 'Error', 'Ignored')),
    AckCode              CHAR(2)       NULL
                         CONSTRAINT CK_Messages_AckCode CHECK (AckCode IN ('AA', 'AE', 'AR', 'CA', 'CE', 'CR')),
    ErrorText            NVARCHAR(1000) NULL
);
GO

--Indexes: support the searches analysts run
CREATE INDEX IX_Messages_ControlId ON dbo.Messages (InterfaceId, ControlId);
CREATE INDEX IX_Messages_PatientMrn ON dbo.Messages (PatientMrn);
CREATE INDEX IX_Messages_Type_Received ON dbo.Messages (MessageType, TriggerEvent, ReceivedAtUtc);

--Filtered indexL only covers error rows
CREATE INDEX IX_Messages_Errors ON dbo.Messages (ReceivedAtUtc) WHERE Status = 'Error';
GO