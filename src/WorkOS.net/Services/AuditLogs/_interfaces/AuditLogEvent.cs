namespace WorkOS
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// The parameters to create an Audit Log Event.
    /// </summary>
    public class AuditLogEvent : BaseOptions
    {
        /// <summary>
        /// Specific activity performed by the actor.
        /// </summary>
        [JsonProperty("action")]
        public string Action { get; set; }

        /// <summary>
        /// ISO-8601 datetime at which the Event happened.
        /// </summary>
        [JsonProperty("occurred_at")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime OccurredAt { get; set; }

        /// <summary>
        /// Actor of the event.
        /// </summary>
        [JsonProperty("actor")]
        public AuditLogEventActor Actor { get; set; }

        /// <summary>
        /// Targets of the event.
        /// </summary>
        [JsonProperty("targets")]
        public List<AuditLogEventTarget> Targets { get; set; }

        /// <summary>
        /// Context of the event.
        /// </summary>
        [JsonProperty("context")]
        public AuditLogEventContext Context { get; set; }

        /// <summary>
        /// Version of the event.
        /// </summary>
        [JsonProperty("version")]
        public int Version { get; set; }

        /// <summary>
        /// Arbitrary key-value data containing information associated with the event.
        /// </summary>
        [JsonProperty("metadata")]
        public Dictionary<string, string> Metadata { get; set; }
    }
}
