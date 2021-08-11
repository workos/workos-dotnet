namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// Contains information about a WorkOS Organization record.
    /// </summary>
    public class Organization
    {
        /// <summary>
        /// Description of the record.
        /// </summary>
        [JsonProperty("object")]
        public const string Object = "organization";

        /// <summary>
        /// The Organization's identifier.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The Organization's name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The Organization's domains.
        /// </summary>
        [JsonProperty("domains")]
        public OrganizationDomain[] Domains { get; set; }

        /// <summary>
        /// The timestamp of when the Organization was created.
        /// </summary>
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        /// <summary>
        /// The timestamp of when the Organization was updated.
        /// </summary>
        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }
    }
}
