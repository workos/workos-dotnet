namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to fetch Connections.
    /// </summary>
    public class ListConnectionsOptions : ListOptions
    {
        /// <summary>
        /// Domain of a <see cref="Connection"/>. Can be empty.
        /// </summary>
        [JsonProperty("domain")]
        public string Domain { get; set; }

        /// <summary>
        /// Searchable text for a <see cref="Connection"/>. Can be empty.
        /// </summary>
        [JsonProperty("connection_type")]
        public string ConnectionType { get; set; }
    }
}
