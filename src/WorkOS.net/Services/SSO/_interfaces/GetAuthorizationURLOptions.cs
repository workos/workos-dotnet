namespace WorkOS
{
    using Newtonsoft.Json;

    public class GetAuthorizationURLOptions : BaseOptions
    {
        [JsonProperty("response_type")]
        public const string ResponseType = "code";

        [JsonProperty("client_id")]
        public string ClientId;

        [JsonProperty("domain")]
        public string Domain;

        [JsonProperty("provider")]
        public ConnectionType? Provider;

        [JsonProperty("redirect_uri")]
        public string RedirectURI;

        [JsonProperty("state")]
        public string State;
    }
}
