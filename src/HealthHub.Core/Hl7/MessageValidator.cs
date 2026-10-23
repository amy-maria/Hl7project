
using System.Text.RegularExpressions;


namespace HealthHub.Core.Hl7;

/// <summary>
/// This object carries the environment information.
/// It tells the rules if the message is standard or high-priority Emergency.
/// </summary>
public record ValidationContext(bool IsEmergency);

/// <summary>
/// The "Contract" for a validation rule. 
/// </summary>
public interface IValidationRule
{
    ValidationResult Validate(Hl7Message msg, ValidationContext context);
}

/// <summary>
/// The Engine that runs the rules.
/// </summary>
public class MessageValidator
{
    private readonly List<IValidationRule> _rules = new();

    public void AddRule(IValidationRule rule) => _rules.Add(rule);

    public ValidationResult Validate(Hl7Message msg, ValidationContext context)
    {
        foreach (var rule in _rules)
        {
            var result = rule.Validate(msg, context);
            if (!result.IsAccepted) return result;
        }
        return ValidationResult.Accept;
    }
}

// --- CONCRETE RULE IMPLEMENTATIONS ---

/// <summary>
/// Rule: Checks if a specific segment (like PID or MSH) exists.
/// </summary>
public class RequiredSegmentRule : IValidationRule
{
    private readonly string _segmentName;
    public RequiredSegmentRule(string segmentName) => _segmentName = segmentName;

    public ValidationResult Validate(Hl7Message msg, ValidationContext context)
    {
        if (msg.GetSegment(_segmentName) == null)
            return new ValidationResult("AE", 100, "Missing Segment", _segmentName, null, $"{_segmentName} segment is required.");
        
        return ValidationResult.Accept;
    }
}

/// <summary>
/// Rule: Checks if a field matches a pattern (e.g., Patient ID).
/// </summary>
public class RegexFieldRule : IValidationRule
{
    private readonly string _segmentName;
    private readonly int _fieldNumber;
    private readonly string _pattern;

    public RegexFieldRule(string segment, int field, string pattern)
    {
        _segmentName = segment;
        _fieldNumber = field;
        _pattern = pattern;
    }
    
    public ValidationResult Validate(Hl7Message msg, ValidationContext context)
    {
        // BYPASS: If this is an emergency, we ignore formatting errors.
        if (context.IsEmergency) return ValidationResult.Accept;

        var segment = msg.GetSegment(_segmentName);
        if (segment == null) return ValidationResult.Accept;

        var value = segment.GetComponent(_fieldNumber, 1);

        if (!Regex.IsMatch(value, _pattern))
        {
            return new ValidationResult("AE", 101, "Format Error", _segmentName, _fieldNumber, $"Field {_fieldNumber} must match pattern: {_pattern}");
        }
        return ValidationResult.Accept;
    }
}

/// <summary>
/// RULE: Checks if a specific field is not empty or whitespace.
/// </summary>
public class NotEmptyRule : IValidationRule
{
    private readonly string _segmentName;
    private readonly int _fieldNumber;

    public NotEmptyRule(string segmentName, int fieldNumber)
    {
        _segmentName = segmentName;
        _fieldNumber = fieldNumber;
    }

    public ValidationResult Validate(Hl7Message msg, ValidationContext context)
    {
        // BYPASS: If this is an emergency, we allow fields to be empty.
        if (context.IsEmergency) return ValidationResult.Accept;

        var segment = msg.GetSegment(_segmentName);
        if (segment == null) return ValidationResult.Accept;

        var value = segment.GetField(_fieldNumber);

        if (string.IsNullOrWhiteSpace(value))
        {
            return new ValidationResult("AE", 101, "Required Field Missing", _segmentName, _fieldNumber, $"Field {_fieldNumber} cannot be empty.");
        }
        return ValidationResult.Accept;
    }
}

/// <summary>
/// RULE: Checks if a field value exists within a specific set of allowed values.
/// </summary>
public class AllowedValuesRule : IValidationRule
{
    private readonly string _segmentName;
    private readonly int _fieldNumber;
    private readonly string[] _allowedValues;

    public AllowedValuesRule(string segment, int field, string[] allowedValues)
    {
        _segmentName = segment;
        _fieldNumber = field;
        _allowedValues = allowedValues;
    }

    public ValidationResult Validate(Hl7Message msg, ValidationContext context)
    {
        var segment = msg.GetSegment(_segmentName);
        if (segment == null) return ValidationResult.Accept;

        var value = segment.GetField(_fieldNumber);

        if (!_allowedValues.Contains(value))
        {
            return new ValidationResult("AE", 102, "Invalid Value", _segmentName, _fieldNumber, $"Value '{value}' is not allowed. Allowed: {string.Join(", ", _allowedValues)}");
        }
        return ValidationResult.Accept;
    }
}
