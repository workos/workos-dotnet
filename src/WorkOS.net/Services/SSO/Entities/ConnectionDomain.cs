namespace WorkOS
{
    using Newtonsoft.Json;

    public class ConnectionDomain
    {
        [JsonProperty("object")]
        public const string Object = "connection_domain";

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }
    }
}
