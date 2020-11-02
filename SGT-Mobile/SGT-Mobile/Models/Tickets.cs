namespace SGTMobile.Models
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Xamarin.Forms;

    public partial class Tickets
    {
        [JsonProperty("ID")]
        public int Id { get; set; }

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

        [JsonProperty("Site_Name")]
        public SiteName SiteName { get; set; }

        [JsonProperty("Report_Status_Mobile")]
        public string Report_Status_Mobile { get; set; }

        [JsonProperty("Service_Details_short_Activity")]
        public string Service_Details { get; set; }

        [JsonProperty("POC_1")]
        public string POC_1 { get; set; }

        [JsonProperty("Phone_POC_1")]
        public string Phone_POC_1 { get; set; }

        [JsonProperty("Email_POC_1")]
        public string Email_POC_1 { get; set; }

        [JsonProperty("Final_User")]
        public string Final_User { get; set; }

        [JsonProperty("Final_User_Phone")]
        public string Final_User_Phone { get; set; }

        [JsonProperty("Final_User_Email")]
        public string Final_User_Email { get; set; }

        [JsonProperty("Client_Intertal")]
        public string ClientIntertal { get; set; }

        [JsonProperty("HasAttachment")]
        public bool HasAttachment { get; set; }

        public string color { get; set; }
    }

    public partial class SiteName
    {
        [JsonProperty("ID")]
        public int Id { get; set; }

        [JsonProperty("Nombre_Sitio")]
        public string NombreSitio { get; set; }

        [JsonProperty("Tipo")]
        public string Tipo { get; set; }

        [JsonProperty("Direccion")]
        public string Direccion { get; set; }

        [JsonProperty("URL_Map")]
        public string UrlMap { get; set; }
    }

    public partial class Tickets
    {
        public static List<Tickets> FromJson(string json) => JsonConvert.DeserializeObject<List<Tickets>>(json, SGTMobile.Models.Converter.Settings);
    }

    public partial class Tickets
    {
        public static Tickets FromJsonUnique(string json) => JsonConvert.DeserializeObject<Tickets>(json, SGTMobile.Models.Converter.Settings);
    }

    public partial class Tickets
    {
        public static string ToJson(List<Tickets> self) => JsonConvert.SerializeObject(self, SGTMobile.Models.Converter.Settings);
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
