using System.Text.Json;
using System.Text.Json.Serialization;

namespace EventOutcomes
{
    public static class EventSerializer
    {
        public static string Serialize(object e) => JsonSerializer.Serialize(e, e.GetType(), new JsonSerializerOptions { Converters = { new JsonStringEnumConverter(), }, });
    }
}
