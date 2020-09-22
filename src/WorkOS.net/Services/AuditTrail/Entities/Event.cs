namespace WorkOS
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Contains information about a WorkOS Event record.
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Description of the record.
        /// </summary>
        [JsonProperty("object")]
        public const string Object = "event";

        /// <summary>
        /// The Event's identifier.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Specific activity performed by the actor.
        /// </summary>
        [JsonProperty("action")]
        public EventAction Action { get; set; }

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
        /// Latitude for where the Event originated.
        /// </summary>
        [JsonProperty("latitude")]
        public string Latitude { get; set; }

        /// <summary>
        /// Identifier for where the Event originated.
        /// </summary>
        [JsonProperty("location")]
        public string Location { get; set; }

        /// <summary>
        /// Longitude for where the Event originated.
        /// </summary>
        [JsonProperty("longitude")]
        public string Longitude { get; set; }

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

        /// <summary>
        /// Corresponding CRUD category of the Event.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
