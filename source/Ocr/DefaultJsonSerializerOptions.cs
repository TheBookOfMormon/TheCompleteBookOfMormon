using System.Text.Json;

namespace Ocr;

internal static class DefaultJsonSerializerOptions
{
    public static readonly JsonSerializerOptions Value =
        new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
}
