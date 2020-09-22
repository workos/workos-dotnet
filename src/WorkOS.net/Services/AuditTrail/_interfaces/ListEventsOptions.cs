namespace WorkOS
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// The parameters to fetch Audit Trail Events.
    /// </summary>
    public class ListEventsOptions : ListOptions
    {
        /// <summary>
        /// Specific activity performed by the actor.
        /// </summary>
        [JsonProperty("action")]
        public List<string> Action { get; set; }

        /// <summary>
        /// Corresponding CRUD category of the Event.
        /// </summary>
        [JsonProperty("action_type")]
        public List<string> ActionType { get; set; }

        /// <summary>
        /// Unique identifier of the entity performing the action.
        /// </summary>
        [JsonProperty("actor_id")]
        public List<string> ActorId { get; set; }

        /// <summary>
        /// Display name of the entity performing the action.
        /// </summary>
        [JsonProperty("actor_name")]
        public List<string> ActorName { get; set; }

        /// <summary>
        /// A single domain containing related members.
        /// </summary>
        [JsonProperty("group")]
        public List<string> Group { get; set; }

        /// <summary>
        /// Unique identifier of the object or resource being acted upon.
        /// </summary>
        [JsonProperty("target_id")]
        public List<string> TargetId { get; set; }

        /// <summary>
        /// Display name of the object or resource that is being acted upon.
        /// </summary>
        [JsonProperty("target_name")]
        public List<string> TargetName { get; set; }

        /// <summary>
        /// ISO-8601 datetime of when an event occurred.
        /// </summary>
        [JsonProperty("occurred_at")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime? OccurredAt { get; set; }

        /// <summary>
        /// ISO-8601 datetime of when an event occurred after.
        /// </summary>
        [JsonProperty("occurred_at_gt")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime? OccurredAtGt { get; set; }

        /// <summary>
        /// ISO-8601 datetime of when an event occurred at or after.
        /// </summary>
        [JsonProperty("occurred_at_gte")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime? OccurredAtGte { get; set; }

        /// <summary>
        /// ISO-8601 datetime of when an event occurred before.
        /// </summary>
        [JsonProperty("occurred_at_lt")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime? OccurredAtLt { get; set; }

        /// <summary>
        /// ISO-8601 datetime of when an event occured at or before.
        /// </summary>
        [JsonProperty("occurred_at_lte")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime? OccurredAtLte { get; set; }

        /// <summary>
        /// Keyword search.
        /// </summary>
        [JsonProperty("search")]
        public string Search { get; set; }
    }
}
