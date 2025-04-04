using System.Text.Json;
using System.Text.Json.Serialization;
using BehaviorNets.Json;

namespace BehaviorNets.Drivers.Drakvuf;

internal static class JsonDefaults
{
    internal static readonly JsonSerializerOptions ApiSerializerOptions = new()
    {
        PropertyNamingPolicy = SnakeCasePolicy.Instance,
        Converters = {new JsonStringEnumConverter(SnakeCasePolicy.Instance)},
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    internal static readonly JsonSerializerOptions LogSerializerOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    internal static readonly JsonSerializerOptions DrakRunLogSerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = AllLowerCasePolicy.Instance,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}