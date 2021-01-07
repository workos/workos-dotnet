namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// Contains information about a WorkOS Event Action record.
    /// </summary>
    public class EventAction
    {
        /// <summary>
        /// Description of the record.
        /// </summary>
        [JsonProperty("object")]
        public const string Object = "event_action";

        /// <summary>
        /// The Event Action's identifier.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The name of the Event Action.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The identifier of the Event Action's Environment.
        /// </summary>
        [JsonProperty("environment_id")]
        public string EnvironmentId { get; set; }
    }
}
