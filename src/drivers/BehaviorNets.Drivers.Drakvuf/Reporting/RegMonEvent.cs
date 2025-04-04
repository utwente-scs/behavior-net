using System.Text.Json;

namespace BehaviorNets.Drivers.Drakvuf.Reporting;

public class RegMonEvent : DrakvufEvent
{
    public string Key
    {
        get;
        set;
    }

    public string ValueName
    {
        get;
        set;
    }

    public string Value
    {
        get;
        set;
    }

    public static RegMonEvent FromJson(string json) =>
        JsonSerializer.Deserialize<RegMonEvent>(json, JsonDefaults.LogSerializerOptions);
}