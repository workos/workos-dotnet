namespace WorkOS
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// The parameters to create an Audit Log Export.
    /// </summary>
    public class CreateAuditLogExportOptions : BaseOptions
    {
        /// <summary>
        /// Organization the Audit Log Export belongs to.
        /// </summary>
        [JsonProperty("organization_id")]
        public string OrganizationId { get; set; }

        /// <summary>
        /// Start of the Export's date range.
        /// </summary>
        [JsonProperty("range_start")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime RangeStart { get; set; }

        /// <summary>
        /// End of the Export's date range.
        /// </summary>
        [JsonProperty("range_end")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime RangeEnd { get; set; }

        /// <summary>
        /// Actions that Audit Log Events will be filtered by.
        /// </summary>
        [JsonProperty("actions")]
        public List<string> Actions { get; set; }

        /// <summary>
        /// Actor names that Audit Log Events will be filtered by.
        /// </summary>
        [JsonProperty("actors")]
        [Obsolete("Please use ActorNames instead.")]
        public List<string> Actors { get; set; }

        /// <summary>
        /// Actor names that Audit Log Events will be filtered by.
        /// </summary>
        [JsonProperty("actor_names")]
        public List<string> ActorNames { get; set; }

        /// <summary>
        /// Actor IDs that Audit Log Events will be filtered by.
        /// </summary>
        [JsonProperty("actor_ids")]
        public List<string> ActorIds { get; set; }

        /// <summary>
        /// Target types that Audit Log Events will be filtered by.
        /// </summary>
        [JsonProperty("targets")]
        public List<string> Targets { get; set; }
    }
}
