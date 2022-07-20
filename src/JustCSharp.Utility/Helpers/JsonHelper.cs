using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace JustCSharp.Utility.Helpers;

public static class JsonHelper
{
    public static JavaScriptEncoder Utf8JavaScriptEncoder = JavaScriptEncoder.Create(UnicodeRanges.All);

    public static JsonSerializerOptions Utf8JsonSerializerOptions
    {
        get
        {
            var options = new JsonSerializerOptions();
            SetUtf8JsonSerializerOptions(options);
            return options;
        }
    }

    public static void SetUtf8JsonSerializerOptions(JsonSerializerOptions options)
    {
        options.Converters.Add(new PreferUtcDateTimeConverter());
        options.Encoder = Utf8JavaScriptEncoder;
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.PropertyNameCaseInsensitive = true;
    }
}