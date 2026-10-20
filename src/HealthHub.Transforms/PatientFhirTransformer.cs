using Newtonsoft.Json;
using HealthHub.Core.Hl7;


namespace HealthHub.Transforms;

/// <summary>
/// This class takes an HL7 v2 message and maps it to a FHIR Patient Resource (JSON).
/// </summary>
/// 

public class PatientFhirTransformer
{
    public string Transform(Hl7Message message)
    {
        var pid = message.GetSegment("PID");

        if (pid == null)
            throw new ArgumentException("Message must contain a PID segment to create a Patient resource.");
        //Build the FHIR obj structure
        var fhirPatient = new
        {
            resourceType = "Patient",
            //Map PID-3 (Patient ID)
            id = pid.GetComponent(3,1).Replace(" ", ""),
            name = new[]
            {
                new
                {
                    family = pid.GetComponent(5, 1), //PID-5 component 1
                    given = new[] {pid.GetComponent(5, 2) } //PID-2 component 2
                }
            },
            gender = MapGender(pid.GetField(8)),
            birthDate = FormatDate(pid.GetField(7)),
            address = new[]
            {
                new
                {
                    text = pid.GetField(11) //PID-11
                }
            }
        };
        //convert the obj to a formatted JSON strig
        return JsonConvert.SerializeObject(fhirPatient, Newtonsoft.Json.Formatting.Indented);
    }
     /// <summary>
    /// Maps HL7 Gender codes to FHIR standard strings.
    /// </summary>
    private string MapGender(string hl7Gender)
    {
        return hl7Gender.ToUpper() switch
        {
            "M" => "male",
            "F" => "female",
            "U" => "other",
            _ => "unknown"
        };
    }

 /// <summary>
    /// Converts HL7 Date format (YYYYMMDD) to FHIR format (YYYY-MM-DD).
    /// </summary>
    private string FormatDate(string hl7Date)
    {
        if (string.IsNullOrWhiteSpace(hl7Date) || hl7Date.Length < 8) 
            return "";
        
        // Transforms 20261001 into 2026-10-01
        return $"{hl7Date[..4]}-{hl7Date[4..6]}-{hl7Date[6..8]}";
    }
}
