namespace WorkOS
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// The parameters to create an Audit Log Event.
    /// </summary>
    public class CreateAuditLogEventOptions : BaseOptions
    {
        /// <summary>
        /// Organization the Audit Log Event belongs to.
        /// </summary>
        [JsonProperty("organization_id")]
        public string OrganizationId { get; set; }

        /// <summary>
        /// The Audit Log Event.
        /// </summary>
        [JsonProperty("event")]
        public AuditLogEvent Event { get; set; }
    }
}
