using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebAPI.Utils;

public class JsonDefaults
{
    public static JsonSerializerOptions DefaultConfigs()
    {
        return new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
#if DEBUG
            WriteIndented = true,
#endif
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            Converters = {
                new JsonStringEnumConverter(),
                //new ObjectToLongConverter(),
                //new TypeJsonConverter(),
                //new GuidConverter(),
                //new NullableGuidConverter(),
                //new DatabaseJsonConverterFactory(),
                //new SyncEntityConverter()
            },
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            MaxDepth = 64,
            IgnoreReadOnlyProperties = true
        };
    }
}
