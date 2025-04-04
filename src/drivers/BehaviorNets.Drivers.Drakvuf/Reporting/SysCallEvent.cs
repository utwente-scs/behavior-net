using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BehaviorNets.Drivers.Drakvuf.Reporting;

public class SysCallEvent : DrakvufEvent
{
    public string Module
    {
        get;
        set;
    }

    [JsonPropertyName("vCPU")]
    public int VCPU
    {
        get;
        set;
    }

    public string CR3
    {
        get;
        set;
    }

    public uint Syscall
    {
        get;
        set;
    }

    public uint NArgs
    {
        get;
        set;
    }

    public List<JsonProperty> Arguments
    {
        get;
    } = new();

    public static SysCallEvent FromJson(string json)
    {
        var result = new SysCallEvent();

        // Ugly, but necessary since the format of a syscall entry is kinda strange
        // in the sense that the arguments are flat mapped into the entry itself, rather
        // than in a list of arguments like in the apimon report.

        // We also don't use a switch(property.Name). This is intentional because property.Name allocates a
        // new string, whereas NameEquals does a ReadOnlySpan<T> comparison instead.

        var document = JsonDocument.Parse(json);
        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (property.NameEquals("Plugin")
                || property.NameEquals("UserName")
                || property.NameEquals("UserId")
                || property.NameEquals("ProcessName")
                || property.NameEquals("EventUID"))
            {
                // Ignore, save allocations.
            }
            else if (property.NameEquals("TimeStamp"))
            {
                result.TimeStamp = property.Value.GetString();
            }
            else if (property.NameEquals("PID"))
            {
                result.PID = property.Value.GetUInt32();
            }
            else if (property.NameEquals("PPID"))
            {
                result.PPID = property.Value.GetUInt32();
            }
            else if (property.NameEquals("TID"))
            {
                result.TID = property.Value.GetUInt32();
            }
            else if (property.NameEquals("Method"))
            {
                result.Method = property.Value.GetString();
            }
            else if (property.NameEquals("Module"))
            {
                result.Module = property.Value.GetString();
            }
            else if (property.NameEquals("vCPU"))
            {
                result.VCPU = property.Value.GetInt32();
            }
            else if (property.NameEquals("CR3"))
            {
                result.CR3 = property.Value.GetString();
            }
            else if (property.NameEquals("Syscall"))
            {
                result.Syscall = property.Value.GetUInt32();
            }
            else if (property.NameEquals("NArgs"))
            {
                result.NArgs = property.Value.GetUInt32();
            }
            else
            {
                result.Arguments.Add(property);
            }
        }

        return result;
    }
}