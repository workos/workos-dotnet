namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// Contains information about a WorkOS Organization Domain record.
    /// </summary>
    public class OrganizationDomain
    {
        /// <summary>
        /// Description of the record.
        /// </summary>
        [JsonProperty("object")]
        public const string Object = "organization_domain";

        /// <summary>
        /// The Organization Domain's identifier.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The domain value.
        /// </summary>
        [JsonProperty("domain")]
        public string Domain { get; set; }
    }
}
