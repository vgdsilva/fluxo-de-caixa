using System.Text.Json;

namespace WebAPI.Entities;

public class AuditLogChanges
{
    public Dictionary<string, object?> Properties { get; init; }

    public AuditLogChanges(Dictionary<string, object?> properties)
    {
        Properties = new Dictionary<string, object?>(properties);
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(Properties);
    }

    public static AuditLogChanges FromJson(string json)
    {
        var properties = JsonSerializer.Deserialize<Dictionary<string, object?>>(json) ?? new();

        return new AuditLogChanges(properties);
    }
}
