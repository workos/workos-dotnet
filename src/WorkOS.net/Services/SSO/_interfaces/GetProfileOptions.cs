namespace WorkOS
{
    using Newtonsoft.Json;

    public class GetProfileOptions : BaseOptions
    {
        [JsonProperty("grant_type")]
        public string GrantType = "authorization_code";

        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }
    }
}
