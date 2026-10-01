USE HealthHub;

SELECT MessageId, SendingApplication, SendingFacility, MessageDateTime, MessageType, TriggerEvent,
       MessageStructure, ControlId, ProcessingId, VersionId, PatientMrn, Status, AckCode
FROM dbo.Messages
ORDER BY MessageId DESC;