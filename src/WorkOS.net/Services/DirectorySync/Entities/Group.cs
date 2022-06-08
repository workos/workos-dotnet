namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// Contains information about a WorkOS Directory Group record.
    /// </summary>
    public class Group
    {
        /// <summary>
        /// Description of the record.
        /// </summary>
        [JsonProperty("object")]
        public const string Object = "directory_group";

        /// <summary>
        /// The Group's identifier.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The Identity Provider identifier.
        /// </summary>
        [JsonProperty("idp_id")]
        public string IdpId { get; set; }

        /// <summary>
        /// The Group's name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The timestamp of when the Factor was created.
        /// </summary>
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        /// <summary>
        /// The timestamp of when the Factor will expire.
        /// </summary>
        [JsonProperty("expires_at")]
        public string ExpiresAt { get; set; }
    }
}
