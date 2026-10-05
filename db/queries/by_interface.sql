USE HealthHub;

SELECT m.MessageId, i.Name AS InterfaceName, m.ControlId, m.MessageType, m.TriggerEvent,
       m.Status, m.AckCode, m.ErrorText
FROM dbo.Messages AS m
JOIN dbo.Interfaces AS i ON i.InterfaceId = m.InterfaceId
WHERE m.MessageId >= 10001
ORDER BY m.MessageId;
