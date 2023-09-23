using System;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace CommonEx;

public static class JsonExtension
{
    private static readonly JsonSerializerOptions SerializerOptions;

    static JsonExtension()
    {
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, //添加中文utf8编码支持(非严格模式)
            PropertyNameCaseInsensitive = true, //反序列化不区分大小写
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase, //支持CamelCase
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase, // 键值对驼峰命名
        };

        //添加相关类型序列化转换器
        options.Converters.Add(new GenericJsonConverter<DateTime>());
        options.Converters.Add(new GenericJsonConverter<DateTime?>());
        SerializerOptions = options;
    }

    public static string ToJson<T>(this T value)
    {
        if (value == null) return string.Empty;
        return JsonSerializer.Serialize(value, SerializerOptions);
    }

    public static T MapTo<T>(this string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return default(T);
        return JsonSerializer.Deserialize<T>(json, SerializerOptions);
    }
}
