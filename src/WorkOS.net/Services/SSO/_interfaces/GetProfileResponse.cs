namespace WorkOS
{
    using Newtonsoft.Json;

    public class GetProfileResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("profile")]
        public Profile Profile { get; set; }
    }
}
