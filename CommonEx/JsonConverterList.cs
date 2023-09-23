using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CommonEx;

public class GenericJsonConverter<T> : JsonConverter<T>
{
    private static readonly Type StringType = typeof(string);
    private static readonly Type DateTimeType = typeof(DateTime);
    private static readonly Type DateTimeNullType = typeof(DateTime?);
    private static readonly Type BoolType = typeof(bool);
    private static readonly Type BoolNullType = typeof(bool?);
    private static readonly Type IntType = typeof(int);
    private static readonly Type IntNulType = typeof(int?);
    private static readonly Type DoubleType = typeof(double);
    private static readonly Type DoubleNullType = typeof(double?);
    private static readonly Type DecimalType = typeof(decimal);
    private static readonly Type DecimalNullType = typeof(decimal?);
    private static readonly Type LongType = typeof(long);
    private static readonly Type LongNullType = typeof(long?);

    public override bool HandleNull => true;

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            if (typeToConvert == StringType) return (T)(dynamic)(reader.GetString() ?? string.Empty);
            if (typeToConvert == DateTimeType || typeToConvert == DateTimeNullType) return (T)(dynamic)DateTime.Parse(reader.GetString());
            if (typeToConvert == BoolType || typeToConvert == BoolNullType) return (T)(dynamic)reader.GetBoolean();
            if (typeToConvert == IntType || typeToConvert == IntNulType) return (T)(dynamic)reader.GetInt32();
            if (typeToConvert == DoubleType || typeToConvert == DoubleNullType) return (T)(dynamic)reader.GetDouble();
            if (typeToConvert == DecimalType || typeToConvert == DecimalNullType) return (T)(dynamic)reader.GetDecimal();
            if (typeToConvert == LongType || typeToConvert == LongNullType) return (T)(dynamic)reader.GetInt64();
            return default;
        }
        catch
        {
            return default(T);
        }
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        try
        {
            Type typeToConvert = typeof(T);
            if (typeToConvert == StringType) writer.WriteStringValue(value as string ?? string.Empty);
            if (typeToConvert == DateTimeType) writer.WriteStringValue((DateTime)(dynamic)value == default ? string.Empty : ((DateTime)(dynamic)value).ToString("yyyy-MM-dd HH:mm:ss"));
            if (typeToConvert == DateTimeNullType) writer.WriteStringValue((value as DateTime?)?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty);
            if (typeToConvert == BoolType || typeToConvert == BoolNullType) writer.WriteBooleanValue((value as bool?) ?? false);
            if (typeToConvert == IntType || typeToConvert == IntNulType) writer.WriteNumberValue((value as int?) ?? default);
            if (typeToConvert == DoubleType || typeToConvert == DoubleNullType) writer.WriteNumberValue((value as double?) ?? default);
            if (typeToConvert == DecimalType || typeToConvert == DecimalNullType) writer.WriteNumberValue((value as decimal?) ?? default);
            if (typeToConvert == LongType || typeToConvert == LongNullType) writer.WriteNumberValue((value as long?) ?? default);
        }
        catch
        {
            writer.WriteStringValue(string.Empty);
        }
    }
}