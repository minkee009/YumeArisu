using System;
using System.Text.Json;

namespace YumeArisu.Core.Utility;

public static class JsonMetaParser
{
    private static readonly JsonSerializerOptions _options = new()
    {
        IncludeFields = true,                        // public 필드 매핑 허용
        PropertyNameCaseInsensitive = true,          // 대소문자 구분 안 함 (유저가 vertbodypath 라고 소문자로 써도 인식)
        ReadCommentHandling = JsonCommentHandling.Skip, // 파일 안의 주석(//) 허용 및 무시
        AllowTrailingCommas = true                   // 마지막 항목 뒤에 쉼표(,)가 남아있어도 에러 안 내고 허용
    };

    public static T Parse<T>(string json) => JsonSerializer.Deserialize<T>(json, _options)!;
}