using System.Text.Encodings.Web;
using System.Text.Json;

namespace CoreApi.Extensions
{
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
                //WriteIndented = true // 是否美化json格式
            };

            //添加相关类型序列化转换器
            options.Converters.Add(new DateTimeConverter());
            options.Converters.Add(new DateTimeNullableConverter());
            SerializerOptions = options;
        }

        public static string ToJson<T>(this T value)
        {
            return JsonSerializer.Serialize(value, SerializerOptions);
        }

        public static T MapTo<T>(this string json)
        {
            return JsonSerializer.Deserialize<T>(json, SerializerOptions);
        }
    }
}