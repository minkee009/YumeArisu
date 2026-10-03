using System.Text.Json;
using System.Text.Json.Serialization;

namespace YumeArisu.Core.Utility;

public static class JsonMetaWriter
{
    private static readonly JsonSerializerOptions _options = new()
    {
        IncludeFields = true,                                         // Rectangle<float>, Vector2D<float>의 필드
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull  // Duration/Event 같은 선택 항목 생략
    };

    public static string ToJson<T>(T value) => JsonSerializer.Serialize(value, _options);

    public static string WithSchema(string json, string schemaUrl)
        => json.Insert(1, $"\n  \"$schema\": \"{schemaUrl}\",");
}