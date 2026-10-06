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
    public int FieldCount =>_fields.Length -1;
    /// <summary>Returns the whole field(all repeitions) or "" if not present
    /// </summary>
    public string GetField(int fieldNumber) => 
        fieldNumber < _fields.Length ? _fields[fieldNumber] : "";
    ///<summary>Returns one component of the first repetition of the field </summary>
    /// 
    /// Returns each repitition for a field as its own string
    public string[] GetRepetitions(int fieldNumber)
    {
        //gets the raw text of the field
        var field = GetField(fieldNumber);

        //empty field has zero repetitions
        if (field == "")
            return [];

        //returns MSH-1/MSH-2 whole as a single item, never split. 
        if (IsDelimiterField(fieldNumber))
            return [field];
        
        //handles other fields by splitting on ~. delims.Repetitions message defines its own delmiters
        return field.Split(_delims.Repetition);
    }

    //Returns one component 1-based of the first repetition of a field.
    public string GetComponent(int fieldNumber, int componentNumber)
        {
            //MSH-1 and MSH-2 have no components;whole value treated as 1 component
            if (IsDelimiterField(fieldNumber))
                return componentNumber == 1 ? GetField(fieldNumber) : "";

            // resuse GetRepetitions instead of splitting on ~ a second tme
            //firstordefault returns first item or null  
            var firstRepetition = GetRepetitions(fieldNumber).FirstOrDefault() ?? "";

            //MSH-1 and MSH-2 hold the delimiter characters and must never be split.
            //var firstRepetition = GetField(fieldNumber).Split(_delims.Repetition)[0];
            var components = firstRepetition.Split(_delims.Component);
            return componentNumber <= components.Length ? components[componentNumber -1] : "";
        }   
        // MSH-1 and MSH-2 are delimited characters themselves; they must never be split 
        //IsDelimiterField is helper method. Private becaue nothing outside class needs it. 
            private bool IsDelimiterField(int fieldNumber) => Name == "MSH" && fieldNumber <= 2;
}
