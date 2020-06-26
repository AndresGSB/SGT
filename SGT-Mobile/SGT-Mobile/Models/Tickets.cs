namespace SGTMobile.Models
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class Tickets
    {
        [JsonProperty("ID")]
        public long Id { get; set; }

        [JsonProperty("Titulo")]
        public string Titulo { get; set; }

        [JsonProperty("ClientTicket")]
        public string ClientTicket { get; set; }

        [JsonProperty("Client")]
        public string Client { get; set; }

        [JsonProperty("Account")]
        public string Account { get; set; }

        [JsonProperty("Service_Date")]
        public DateTimeOffset ServiceDate { get; set; }
    }

    public partial class Tickets
    {
        public static List<Tickets> FromJson(string json) => JsonConvert.DeserializeObject<List<Tickets>>(json, SGTMobile.Models.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this List<Tickets> self) => JsonConvert.SerializeObject(self, SGTMobile.Models.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
