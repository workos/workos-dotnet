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
        /// The Group's name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
