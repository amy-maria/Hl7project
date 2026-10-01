using System.Data;
using System.Globalization;

namespace HealthHub.Core.Hl7; 

public static class Hl7DateTime
{  ///list of all date patterns to accept, static readonly=created once and can't be replaced
    private static readonly string[] Formats = ["yyyyMMddHHmmss", "yyyyMMddHHmm", "yyyyMMddHH", "yyyyMMdd"];

    ///parses HL7 datetime, returns null if value is empty or not valid date. Time zone
    /// offsets are dropped
    
    public static DateTime? Parse(string value)
    { ///missing or bad date returns null instead of crashing, db allows for null
    ///NullOrWhiteSpace  handles null or whitespace first
        if (string.IsNullOrWhiteSpace(value))
            return null;

        //keep only digits before any timezone offset or fraction
        //finds +-. or returns -1 if none
        //take the text before cut otherwise keep the whole value

        int cut = value.IndexOfAny(['+', '-', '.']);
        string digits = cut >0 ? value.Substring(0,cut) : value;

        //trys each format until one fits, returns t/f instead of crashing out, returns date/time result alongside t/f answer
        //cultureInfo= parse the same regardless of language and region settings
        if (DateTime.TryParseExact(digits, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
            return result;
        
        return null;
    }
}