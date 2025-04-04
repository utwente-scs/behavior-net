using System.Collections.Generic;
using System.IO;

namespace BehaviorNets.Drivers.Drakvuf.Reporting;

public class RegMonReport
{
    public List<RegMonEvent> Events
    {
        get;
    } = new();
        
    public static RegMonReport FromFile(string filePath)
    {
        using var reader = new StreamReader(filePath);
        return FromReader(reader);
    }

    public static RegMonReport FromJson(string json)
    {
        using var reader = new StringReader(json);
        return FromReader(reader);
    }
        
    public static RegMonReport FromReader(TextReader reader)
    {
        var result = new RegMonReport();

        while (reader.Peek() >= 0)
            result.Events.Add(RegMonEvent.FromJson(reader.ReadLine()));

        return result;
    }
}