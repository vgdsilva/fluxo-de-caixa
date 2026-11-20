using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebAPI.Common.Converters;

/// <summary>Serializa DateOnly sempre como yyyy-MM-dd (útil para datas de lançamento/vencimento).</summary>
public sealed class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const string Format = "yyyy-MM-dd";

    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        DateOnly.ParseExact(reader.GetString()!, Format, CultureInfo.InvariantCulture);

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
}
