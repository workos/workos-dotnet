namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// Describes the domain records associated with a <see cref="Connection"/>.
    /// </summary>
    public class ConnectionDomain
    {
        /// <summary>
        /// Description of the record.
        /// </summary>
        [JsonProperty("object")]
        public const string Object = "connection_domain";

        /// <summary>
        /// Connection Domain identifier.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The Domain for a <see cref="Connection"/> record.
        /// </summary>
        [JsonProperty("domain")]
        public string Domain { get; set; }
    }
}
