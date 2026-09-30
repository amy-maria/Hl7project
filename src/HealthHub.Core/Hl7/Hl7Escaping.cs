using System.Text; //brings in StringBuilder

namespace HealthHub.Core.Hl7;

public static class Hl7Escaping
{
    /// <summary>
    /// Converts HL7 escape sequences (\F\ \S\ \R\ \T\ \E\) back into the characters they represent.
    /// </summary>
    /// Hl7Escaping.Unescape utility code 
    public static string Unescape(string value, Delimiters d)
    {
        // Fast path: nothing to do if there's no escape character at all.
        if (!value.Contains(d.Escape))
            return value;

        var result = new StringBuilder(value.Length);
        int i = 0;

        while (i < value.Length)
        {
            char c = value[i];

            // Ordinary character: copy it and move on.
            if (c != d.Escape)
            {
                result.Append(c);
                i++;
                continue;
            }

            // We're at an escape character. Find the one that closes the sequence.
            int end = value.IndexOf(d.Escape, i + 1);
            if (end == -1)
            {
                // No closing escape character: malformed. Keep the rest unchanged.
                result.Append(value, i, value.Length - i);
                break;
            }

            // The code is whatever sits between the two escape characters, e.g. "T".
            string code = value.Substring(i + 1, end - i - 1);

            switch (code)
            {
                case "F": result.Append(d.Field); break;
                case "S": result.Append(d.Component); break;
                case "R": result.Append(d.Repetition); break;
                case "T": result.Append(d.Subcomponent); break;
                case "E": result.Append(d.Escape); break;
                default:
                    // Sequences we don't handle yet (like \.br\ for line breaks): keep them as-is.
                    result.Append(value, i, end - i + 1);
                    break;
            }

            i = end + 1;   // jump past the closing escape character
        }

        return result.ToString();
    }
}