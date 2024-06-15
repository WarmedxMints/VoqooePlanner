using EliteJournalReader;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Globalization;

namespace VoqooePlanner.Models
{
    public class NavigationRoute
    {
        [JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? Timestamp { get; set; }

        [JsonProperty("event", NullValueHandling = NullValueHandling.Ignore)]
        public string Event { get; set; } = string.Empty;

        [JsonProperty("Route", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<Route> Route { get; set; } = [];

        public static NavigationRoute? FromJson(string json) => JsonConvert.DeserializeObject<NavigationRoute>(json, Converter.Settings);
    }

    public class Route
    {
        [JsonProperty("StarSystem", NullValueHandling = NullValueHandling.Ignore)]
        public string StarSystem { get; set; } = string.Empty;

        [JsonProperty("SystemAddress", NullValueHandling = NullValueHandling.Ignore)]
        public long SystemAddress { get; set; }

        [JsonProperty("StarPos", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(SystemPositionConverter))]
        public SystemPosition StarPos { get; set; }

        [JsonConverter(typeof(ExtendedStringEnumConverter<StarType>))]
        public StarType StarClass { get; set; }
    }

    public static class Serialize
    {
        public static string ToJson(this NavigationRoute self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new()
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
