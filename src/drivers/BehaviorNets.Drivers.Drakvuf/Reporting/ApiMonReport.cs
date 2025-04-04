using System.Collections.Generic;
using System.IO;

namespace BehaviorNets.Drivers.Drakvuf.Reporting;

public class ApiMonReport
{
    public List<ApiMonEvent> Events
    {
        get;
    } = new();

    public static ApiMonReport FromFile(string filePath)
    {
        using var reader = new StreamReader(filePath);
        return FromReader(reader);
    }

    public static ApiMonReport FromJson(string json)
    {
        using var reader = new StringReader(json);
        return FromReader(reader);
    }
        
    public static ApiMonReport FromReader(TextReader reader)
    {
        var result = new ApiMonReport();

        while (reader.Peek() >= 0)
            result.Events.Add(ApiMonEvent.FromJson(reader.ReadLine()));

        return result;
    }
}