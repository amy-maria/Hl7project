namespace HealthHub.Core.Hl7;
//handles MSH counting quirk

public sealed class Segment
{
    private readonly string[] _fields;
    private readonly Delimiters _delims;

    public Segment(string raw, Delimiters delims)
    {
        _delims = delims;
        var fields = raw.Split(delims.Field).ToList();
        //MSH-1 is field separator. Split it off and put it back for index
        //N == MSH-N 

        if (fields[0] == "MSH")
            fields.Insert(1, delims.Field.ToString());
        
        _fields = fields.ToArray();
    } 

    public string Name => _fields[0];
    /// <summary>Returns the whole field(all repeitions) or "" if not present
    /// </summary>
    public string GetField(int fieldNumber) => 
        fieldNumber < _fields.Length ? _fields[fieldNumber] : "";
    ///<summary>Returns one component of the first repetition of the field </summary>
    /// 
    public string GetComponent(int fieldNumber, int componentNumber)
        {
            var firstRepetition = GetField(fieldNumber).Split(_delims.Repetition)[0];
            var components = firstRepetition.Split(_delims.Component);
            return componentNumber <= components.Length ? components[componentNumber -1] : "";

        }   
}
