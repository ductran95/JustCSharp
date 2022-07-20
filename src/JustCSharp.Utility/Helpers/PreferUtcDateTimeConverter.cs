using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JustCSharp.Utility.Helpers;

public class PreferUtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateTime = reader.GetDateTime();

        if (dateTime.Kind == DateTimeKind.Unspecified)
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute,
                dateTime.Second, dateTime.Millisecond, DateTimeKind.Utc);

        return dateTime;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}