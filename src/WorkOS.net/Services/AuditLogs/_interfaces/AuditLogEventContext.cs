namespace WorkOS
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Parameters representing an Audit Log Event context.
    /// </summary>
    public class AuditLogEventContext : BaseOptions
    {
        /// <summary>
        /// IP address of the Audit Log Event.
        /// </summary>
        [JsonProperty("location")]
        public string Location { get; set; }

        /// <summary>
        /// User agent of the Audit Log Event.
        /// </summary>
        [JsonProperty("user_agent")]
        public string UserAgent { get; set; }
    }
}
