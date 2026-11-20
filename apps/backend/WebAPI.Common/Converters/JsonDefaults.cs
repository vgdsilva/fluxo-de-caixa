using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebAPI.Common.Converters;

/// <summary>Opções de JSON únicas para API, outbox e webhooks.</summary>
public static class JsonDefaults
{
    public static readonly JsonSerializerOptions Options = Create();

    public static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new DateOnlyJsonConverter());
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
