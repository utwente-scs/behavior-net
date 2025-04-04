using System.Text;
using System.Text.Json;

namespace BehaviorNets.Json;

/// <summary>
/// Provides an implementation for a snake casing JSON naming policy.
/// </summary>
public class SnakeCasePolicy : JsonNamingPolicy
{
    private SnakeCasePolicy()
    {
    }

    /// <summary>
    /// Gets the singleton instance of this policy.
    /// </summary>
    public static SnakeCasePolicy Instance
    {
        get;
    } = new();

    /// <inheritdoc />
    public override string ConvertName(string name)
    {
        var builder = new StringBuilder();

        foreach (char c in name)
        {
            if (char.IsUpper(c) && builder.Length > 0)
                builder.Append('_');

            builder.Append(char.ToLower(c));
        }

        return builder.ToString();
    }
}