using System.Collections.Generic;
using System.IO;

namespace BehaviorNets.Drivers.Drakvuf.Reporting;

public class DrakRunReport
{
    public List<DrakRunEvent> LogEntries { get; } = new();

    public static DrakRunReport FromFile(string file)
    {
        using var reader = new StreamReader(file);
        return FromReader(reader);
    }

    public static DrakRunReport FromJson(string json)
    {
        using var reader = new StringReader(json);
        return FromReader(reader);
    }

    public static DrakRunReport FromReader(TextReader reader)
    {
        var result = new DrakRunReport();
        while (reader.Peek() >= 0) 
            result.LogEntries.Add(DrakRunEvent.FromJson(reader.ReadLine()));
        return result;
    }

    public string ExtractSampleHash()
    {
        foreach (var entry in LogEntries)
        {
            if (entry.Message.Contains("Sample SHA256: "))
                return entry.Message[15..];
        }

        return null;
    }
}