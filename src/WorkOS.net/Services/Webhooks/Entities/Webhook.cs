// @oagen-ignore-file
namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// Describes the webhook records associated with a webhook event.
    /// </summary>
    public class Webhook
    {
        /// <summary>
        /// Unique identifier for the object.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; } = default!;

        /// <summary>
        /// The event type.
        /// </summary>
        [JsonProperty("event")]
        public string Event { get; set; } = default!;

        /// <summary>
        /// The raw webhook payload data.
        /// </summary>
        [JsonProperty("data")]
        public object? Data { get; set; }

        /// <summary>
        /// The timestamp of when the event was created.
        /// </summary>
        [JsonProperty("created_at")]
        public string? CreatedAt { get; set; }
    }
}
