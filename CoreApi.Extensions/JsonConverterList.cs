using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoreApi.Extensions;

public class DateTimeJsonConverter : JsonConverter<DateTime>
{
    private readonly string format = string.Empty;

    public DateTimeJsonConverter(string timeFormat = "yyyy-MM-dd HH:mm:ss")
    {
        format = timeFormat;
    }

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.TryParse(reader.GetString(), out DateTime value) ? value : default;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value != default ? value.ToString(format) : "");
    }
}

public class DateTimeNullableJsonConverter : JsonConverter<DateTime?>
{
    private readonly string format = string.Empty;

    public DateTimeNullableJsonConverter(string timeFormat = "yyyy-MM-dd HH:mm:ss")
    {
        format = timeFormat;
    }

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.TryParse(reader.GetString(), out DateTime value) ? value : default;
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value?.ToString(format));
    }
}
