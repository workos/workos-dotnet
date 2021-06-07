namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// Contains information about a WorkOS Directory record.
    /// </summary>
    public class Directory
    {
        /// <summary>
        /// Description of the record.
        /// </summary>
        [JsonProperty("object")]
        public const string Object = "directory";

        /// <summary>
        /// The Directory's identifier.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The name of the Directory.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The domain of the Directory.
        /// </summary>
        [JsonProperty("domain")]
        public string Domain { get; set; }

        /// <summary>
        /// The linked state of the Directory.
        /// </summary>
        [JsonProperty("state")]
        public DirectoryState State { get; set; }

        /// <summary>
        /// Type of the directory.
        /// </summary>
        public DirectoryType Type { get; set; }

        /// <summary>
        /// Externally used identifier for the Directory.
        /// </summary>
        [JsonProperty("external_key")]
        public string ExternalKey { get; set; }

        /// <summary>
        /// Identifier for the Directory's Organization.
        /// </summary>
        [JsonProperty("organization_id")]
        public string OrganizationId { get; set; }

        /// <summary>
        /// Identifier for the Directory's Environment.
        /// </summary>
        [JsonProperty("environment_id")]
        public string EnvironmentId { get; set; }
    }
}
