using System.Collections.Generic;
using System.Text.Json;

namespace BehaviorNets.Drivers.Drakvuf.Reporting;

public class ApiMonEvent : DrakvufEvent
{
    public string Event
    {
        get;
        set;
    }

    public string ReturnValue
    {
        get;
        set;
    }

    public List<string> Arguments
    {
        get;
        set;
    }

    public string DllBase
    {
        get;
        set;
    }

    public string DllName
    {
        get;
        set;
    }

    public static ApiMonEvent FromJson(string json) =>
        JsonSerializer.Deserialize<ApiMonEvent>(json, JsonDefaults.LogSerializerOptions);
}