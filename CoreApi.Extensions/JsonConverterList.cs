using System;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoreApi.Extensions
{
    public class DateTimeJsonConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.TryParse(reader.GetString(), out DateTime value) ? value : default;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value != default ? value.ToString("yyyy-MM-dd HH:mm:ss") : "");
        }
    }

    public class DateTimeNullableJsonConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.TryParse(reader.GetString(), out DateTime value) ? value : default;
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value?.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }

    #region DataTableJsonConverter
    public class DataTableJsonConverter : JsonConverter<DataTable>
    {
        public override DataTable Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var jsonDoc = JsonDocument.ParseValue(ref reader);
            var rootElement = jsonDoc.RootElement;
            var dataTable = rootElement.JsonElementToDataTable();
            return dataTable;
        }

        public override void Write(Utf8JsonWriter writer, DataTable value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (DataRow dr in value.Rows)
            {
                writer.WriteStartObject();
                foreach (DataColumn col in value.Columns)
                {
                    var key = col.ColumnName;
                    var valueString = dr[col].ToString().AsSpan();
                    switch (col.DataType.FullName)
                    {
                        case "System.Guid":
                            writer.WriteString(key, valueString);
                            break;
                        case "System.Char":
                        case "System.String":
                            writer.WriteString(key, valueString);
                            break;
                        case "System.Boolean":
                            _ = bool.TryParse(valueString, out bool boolValue);
                            writer.WriteBoolean(key, boolValue);
                            break;
                        case "System.DateTime":
                            var res = DateTime.TryParse(valueString, out DateTime dateValue);
                            writer.WriteString(key, res ? dateValue.ToString("yyyy-MM-dd HH:mm:ss") : "");
                            break;
                        case "System.TimeSpan":
                            var flag = DateTime.TryParse(valueString, out DateTime timeSpanValue);
                            writer.WriteString(key, flag ? timeSpanValue.ToString() : "");
                            break;
                        case "System.Byte":
                        case "System.SByte":
                        case "System.Decimal":
                        case "System.Double":
                        case "System.Single":
                        case "System.Int16":
                        case "System.Int32":
                        case "System.Int64":
                        case "System.UInt16":
                        case "System.UInt32":
                        case "System.UInt64":
                            if (long.TryParse(valueString, out long intValue))
                            {
                                writer.WriteNumber(key, intValue);
                            }
                            else
                            {
                                _ = double.TryParse(valueString, out double doubleValue);
                                writer.WriteNumber(key, doubleValue);
                            }
                            break;
                        default:
                            writer.WriteString(key, valueString);
                            break;
                    }
                }
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }
    }
    #endregion

    #region JsonElementExtension
    internal static class JsonElementExtension
    {
        public static DataTable JsonElementToDataTable(this JsonElement dataRoot)
        {
            var dataTable = new DataTable();
            var firstPass = true;
            foreach (var element in dataRoot.EnumerateArray())
            {
                if (firstPass)
                {
                    foreach (var col in element.EnumerateObject())
                    {
                        var colValue = col.Value;
                        dataTable.Columns.Add(new DataColumn(col.Name, colValue.ValueKind.ValueKindToType(colValue.ToString())));
                    }
                    firstPass = false;
                }

                var row = dataTable.NewRow();
                foreach (var col in element.EnumerateObject())
                {
                    row[col.Name] = col.Value.JsonElementToTypedValue();
                }
                dataTable.Rows.Add(row);
            }
            return dataTable;
        }

        private static Type ValueKindToType(this JsonValueKind valueKind, string value)
        {
            switch (valueKind)
            {
                case JsonValueKind.String:      // 3
                    return typeof(System.String);
                case JsonValueKind.Number:      // 4    
                    if (Int64.TryParse(value, out var intValue))
                        return typeof(System.Int64);
                    return typeof(System.Double);
                case JsonValueKind.True:        // 5
                case JsonValueKind.False:       // 6
                    return typeof(System.Boolean);
                case JsonValueKind.Undefined:   // 0
                    return null;
                case JsonValueKind.Object:      // 1 
                    return typeof(System.Object);
                case JsonValueKind.Array:       // 2
                    return typeof(System.Array);
                case JsonValueKind.Null:        // 7
                    return null;
                default:
                    return typeof(System.Object);
            }
        }

        private static object JsonElementToTypedValue(this JsonElement jsonElement)
        {
            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.Object:      // 1  (these need special handling)?
                case JsonValueKind.Array:       // 2 
                case JsonValueKind.String:      // 3
                    if (jsonElement.TryGetGuid(out Guid guidValue))
                    {
                        return guidValue;
                    }

                    if (jsonElement.TryGetDateTime(out DateTime datetime))
                    {
                        // If an offset was provided, use DateTimeOffset.
                        if (datetime.Kind == DateTimeKind.Local)
                        {
                            if (jsonElement.TryGetDateTimeOffset(out DateTimeOffset datetimeOffset))
                            {
                                return datetimeOffset;
                            }
                        }
                        return datetime;
                    }

                    return jsonElement.ToString();
                case JsonValueKind.Number:      // 4    
                    if (jsonElement.TryGetInt64(out long longValue))
                        return longValue;

                    return jsonElement.GetDouble();
                case JsonValueKind.True:        // 5
                case JsonValueKind.False:       // 6
                    return jsonElement.GetBoolean();
                case JsonValueKind.Undefined:   // 0
                case JsonValueKind.Null:        // 7
                    return null;
                default:
                    return jsonElement.ToString();
            }
        }
    }
    #endregion
}
