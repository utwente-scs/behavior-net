using System.Text.Json;

namespace BehaviorNets.Json;

/// <summary>
/// Provides a JSON naming policy where all names are converted to lowercase (invariant culture).
/// </summary>
public class AllLowerCasePolicy : JsonNamingPolicy
{
    private AllLowerCasePolicy()
    {
    }

    /// <summary>
    /// Gets the singleton instance of this policy.
    /// </summary>
    public static AllLowerCasePolicy Instance
    {
        get;
    } = new();
        
    /// <inheritdoc />
    public override string ConvertName(string name) => name.ToLowerInvariant();
}