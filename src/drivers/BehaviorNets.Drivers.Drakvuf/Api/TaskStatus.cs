using System.Text.Json.Serialization;

namespace BehaviorNets.Drivers.Drakvuf.Api;

public class TaskStatus
{
    [JsonPropertyName("status")]
    public TaskStatusCode StatusCode
    {
        get;
        set;
    }

    [JsonPropertyName("vm_id")]
    public string VMID
    {
        get;
        set;
    }
}