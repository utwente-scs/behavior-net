using System.Text.Json;

namespace BehaviorNets.Drivers.Drakvuf.Reporting;

public class DrakRunEvent
{
    public static DrakRunEvent FromJson(string json)
    {
        return JsonSerializer.Deserialize<DrakRunEvent>(json, JsonDefaults.DrakRunLogSerializerOptions);
    } 
        
    public string LevelName
    {
        get;
        set;
    }

    public double Created
    {
        get;
        set;
    }

    public string Message
    {
        get;
        set;
    }
}