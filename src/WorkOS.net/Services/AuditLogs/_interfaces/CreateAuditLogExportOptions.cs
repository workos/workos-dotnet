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
        public List<string> Actors { get; set; }

        /// <summary>
        /// Target types that Audit Log Events will be filtered by.
        /// </summary>
        [JsonProperty("targets")]
        public List<string> Targets { get; set; }
    }
}
