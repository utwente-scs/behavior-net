using System.Collections.Generic;
using System.IO;

namespace BehaviorNets.Drivers.Drakvuf.Reporting;

public class SysCallReport
{
    public List<SysCallEvent> Events
    {
        get;
    } = new();

    public static SysCallReport FromFile(string filePath)
    {
        using var reader = new StreamReader(filePath);
        return FromReader(reader);
    }

    public static SysCallReport FromJson(string json)
    {
        using var reader = new StringReader(json);
        return FromReader(reader);
    }
        
    public static SysCallReport FromReader(TextReader reader)
    {
        var result = new SysCallReport();

        while (reader.Peek() >= 0)
            result.Events.Add(SysCallEvent.FromJson(reader.ReadLine()));

        return result;
    }
}