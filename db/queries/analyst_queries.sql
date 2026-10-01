USE HealthHub;

SELECT TOP (50)
    m.MessageId,
    i.Name AS InterfaceName,
    CONCAT(m.MessageType, '^', m.TriggerEvent) AS Event,
    m.ControlId,
    m.PatientMrn,
    m.Status,
    m.AckCode,
    m.ReceivedAtUtc
FROM dbo.Messages as m
JOIN dbo.Interfaces AS i ON i.InterfaceID = m.InterfaceId
ORDER BY m.ReceivedAtUtc DESC;

SELECT MessageType, TriggerEvent, COUNT(*) AS MessageCount
FROM dbo.Messages
GROUP BY MessageType, TriggerEvent
ORDER BY MessageCount DESC;

SELECT
   m.MessageId, 
   i.Name AS IntefaceName,
   m.ControlId,
   m.AckCode,
   m.ErrorText,
   m.ReceivedAtUtc
FROM dbo.Messages AS m
JOIN dbo.Interfaces AS i ON i.InterfaceId = m.InterfaceId
WHERE m.Status = 'Error'
ORDER BY m.ReceivedAtUtc;
