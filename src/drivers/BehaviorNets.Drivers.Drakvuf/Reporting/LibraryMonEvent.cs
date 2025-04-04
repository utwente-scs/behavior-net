using System.Text.Json;

namespace BehaviorNets.Drivers.Drakvuf.Reporting;

public class LibraryMonEvent : DrakvufEvent
{
    public string ModuleName
    {
        get;
        set;
    }

    public string ModulePath
    {
        get;
        set;
    }

    public static LibraryMonEvent FromJson(string json) =>
        JsonSerializer.Deserialize<LibraryMonEvent>(json, JsonDefaults.LogSerializerOptions);
}