
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace HealthHub.Core.Hl7;

/// <summary>
/// The "Contract" for a validation rule. 
/// This allows us to add new rules without changing the Validator engine itself.
/// (This follows the Open/Closed Principle).
/// </summary>

public interface IValidationRule
{
    ValidationResult Validate(Hl7Message msg);
}

/// <summary>
    /// Allows us to "plug in" new rules at runtime.
    /// </summary>
    /// 
public class MessageValidator
{
    private readonly List<IValidationRule> _rules = new();
    /// <summary>
    /// Allows us to "plug in" new rules at runtime.
    /// </summary>
    public void AddRule(IValidationRule rule) => _rules.Add(rule);
    public ValidationResult Validate(Hl7Message msg)
    {
        foreach (var rule in _rules)
        {
            var result = rule.Validate(msg);
            //If any rule fails (IsAccepted is false), stop and return that error immediately
            if (!result.IsAccepted) return result;
        
        }
        return ValidationResult.Accept;
    }
}
// ---CONCRETE RULE IMPLEMENTATIONS ---

/// <summary>
/// Rule: Checks if a specific field in a segment matches the RegEx pattern.   Version is component aware so it doesn't get confused by delimiters. (like PID or MSH) exists in the message.
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
    
    public ValidationResult Validate(Hl7Message msg)
    {
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

public class RequiredSegmentRule : IValidationRule
{
    private readonly string _segmentName;
    public RequiredSegmentRule(string segmentName) => _segmentName = segmentName;
    public ValidationResult Validate(Hl7Message msg)
    {
        if (msg.GetSegment(_segmentName) == null)
            return new ValidationResult("AE", 100, "Missing Segment", _segmentName, null, $"{_segmentName} segment is required.");
        return ValidationResult.Accept;
    }   
}
/// <summary>
/// RULE: Checks if a specific field is not empty or whitespace.
/// </summary
public class NotEmptyRule : IValidationRule
{
    private readonly string _segmentName;
    private readonly int _fieldNumber;
    public NotEmptyRule(string segmentName, int fieldNumber)
    {
        _segmentName = segmentName;
        _fieldNumber = fieldNumber;

    }
    public ValidationResult Validate(Hl7Message msg)
    {
        var segment = msg.GetSegment(_segmentName);
        //If the segment is missing, return Accept here. RequiredSegmentRule is responsible for reporting missing segments.
        if (segment == null) return ValidationResult.Accept;

        var value = segment.GetField(_fieldNumber);

        if (string.IsNullOrWhiteSpace (value))
        {
            return new ValidationResult("AE", 101, "Required Field Missing", _segmentName, _fieldNumber, $"Field {_fieldNumber} cannot be empty.");
        
        }
        return ValidationResult.Accept;

    }
}
