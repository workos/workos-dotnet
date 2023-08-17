namespace WorkOS
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Contains information about a WorkOS Connection record.
    /// </summary>
    public class Connection
    {
        /// <summary>
        /// Description of the record.
        /// </summary>
        [JsonProperty("object")]
        public const string Object = "connection";

        /// <summary>
        /// The Connection's identifier.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The unique identifier for the Organization in which the Connection resides.
        /// </summary>
        [JsonProperty("organization_id")]
        public string OrganizationId { get; set; }

        /// <summary>
        /// The name of the Connection.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The type of the Connection.
        /// </summary>
        [JsonProperty("connection_type")]
        public ConnectionType? Type { get; set; }

        /// <summary>
        /// The linked state of the Connection.
        /// </summary>
        [JsonProperty("state")]
        public ConnectionState State { get; set; }

        /// <summary>
        /// The linked status of the Connection.
        /// </summary>
        [JsonProperty("status")]
        [ObsoleteAttribute("The Status property is obsolete. Please use State instead.", false)]
        public ConnectionStatus Status { get; set; }

        /// <summary>
        /// Domain records for the Connection.
        /// </summary>
        [JsonProperty("domains")]
        public ConnectionDomain[] Domains { get; set; }

        /// <summary>
        /// The timestamp of when the Connection was created.
        /// </summary>
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        /// <summary>
        /// The timestamp of when the Connection was updated.
        /// </summary>
        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }
    }
}
