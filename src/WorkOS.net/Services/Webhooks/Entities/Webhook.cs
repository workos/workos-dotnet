namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// Describes the webhook records associated with a Webhook Event.
    /// </summary>
    public class Webhook
    {
        /// <summary>
        /// Unique identifier for the object.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The event of webhook.
        /// </summary>
        [JsonProperty("event")]
        public string Event { get; set; }

        /// <summary>
        /// The raw data webhook.
        /// </summary>
        [JsonProperty("data")]
        public object Data { get; set; }

        /// <summary>
        /// The created_at timestamp of the event.
        /// </summary>
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }
    }
}
