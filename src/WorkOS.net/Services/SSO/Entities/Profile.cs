namespace WorkOS
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class Profile
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("idp_id")]
        public string IdpId { get; set; }

        [JsonProperty("connection_type")]
        public ConnectionType? ConnectionType { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("raw_attributes")]
        public Dictionary<string, string> RawAttributes { get; set; }
    }
}
