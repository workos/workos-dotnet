namespace WorkOS
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// The parameters to create an Audit Trail Event.
    /// </summary>
    public class CreateEventOptions : BaseOptions
    {
        /// <summary>
        /// Specific activity performed by the actor.
        /// </summary>
        [JsonProperty("action")]
        public string Action { get; set; }

        /// <summary>
        /// Corresponding CRUD category of the Event.
        /// </summary>
        [JsonProperty("action_type")]
        public string ActionType { get; set; }

        /// <summary>
        /// Unique identifier of the entity performing the action.
        /// </summary>
        [JsonProperty("actor_id")]
        public string ActorId { get; set; }

        /// <summary>
        /// Display name of the entity performing the action.
        /// </summary>
        [JsonProperty("actor_name")]
        public string ActorName { get; set; }

        /// <summary>
        /// A single domain containing related members.
        /// </summary>
        [JsonProperty("group")]
        public string Group { get; set; }

        /// <summary>
        /// Identifier for where the Event originated.
        /// </summary>
        [JsonProperty("location")]
        public string Location { get; set; }

        /// <summary>
        /// Arbitrary key-value data containing information associated with the Event.
        /// </summary>
        [JsonProperty("metadata")]
        public Dictionary<string, string> Metadata { get; set; }

        /// <summary>
        /// ISO-8601 datetime at which the Event happened.
        /// </summary>
        [JsonProperty("occurred_at")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime OccurredAt { get; set; }

        /// <summary>
        /// Unique identifier of the object or resource being acted upon.
        /// </summary>
        [JsonProperty("target_id")]
        public string TargetId { get; set; }

        /// <summary>
        /// Display name of the object or resource that is being acted upon.
        /// </summary>
        [JsonProperty("target_name")]
        public string TargetName { get; set; }
    }
}
