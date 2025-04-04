using System.Collections.Generic;
using System.IO;

namespace BehaviorNets.Drivers.Drakvuf.Reporting;

public class LibraryMonReport
{
    public List<LibraryMonEvent> Events
    {
        get;
    } = new();

    public static LibraryMonReport FromFile(string filePath)
    {
        using var reader = new StreamReader(filePath);
        return FromReader(reader);
    }

    public static LibraryMonReport FromJson(string json)
    {
        using var reader = new StringReader(json);
        return FromReader(reader);
    }
        
    public static LibraryMonReport FromReader(TextReader reader)
    {
        var result = new LibraryMonReport();

        while (reader.Peek() >= 0)
            result.Events.Add(LibraryMonEvent.FromJson(reader.ReadLine()));

        return result;
    }
}