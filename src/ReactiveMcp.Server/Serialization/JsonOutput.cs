using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReactiveMcp.Server.Serialization;

/// <summary>
/// Provides stable JSON serialization for MCP tool and resource output.
/// </summary>
public static class JsonOutput
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// Serializes a value to JSON.
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <returns>A JSON string.</returns>
    public static string Serialize(object value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return JsonSerializer.Serialize(value, Options);
    }
}
