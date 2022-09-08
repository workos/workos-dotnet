namespace WorkOS
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Contains information about a WorkOS Audit Log Export record.
    /// </summary>
    public class AuditLogExport
    {
        /// <summary>
        /// Description of the record.
        /// </summary>
        [JsonProperty("object")]
        public const string Object = "audit_log_export";

        /// <summary>
        /// The unique identifier of the Audit Log Export.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The state of the Audit Log Export.
        /// </summary>
        [JsonProperty("state")]
        public AuditLogExportState State { get; set; }

        /// <summary>
        /// The URL of the CSV export file.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// The timestamp of when the Audit Log Export was created.
        /// </summary>
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        /// <summary>
        /// The timestamp of when the Audit Log Export was updated.
        /// </summary>
        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }
    }
}
