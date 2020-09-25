namespace WorkOS
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Contains information about a WorkOS Passwordless Session record.
    /// </summary>
    public class PasswordlessSession
    {
        /// <summary>
        /// Description of the record.
        /// </summary>
        [JsonProperty("object")]
        public const string Object = "passwordless_session";

        /// <summary>
        /// The Passwordless Session's identifier.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The e-mail of the user to authenticate.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Datetime at which the Passwordless Session expires.
        /// </summary>
        [JsonProperty("expires_at")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// The URL to confirm the Passwordless Session.
        /// </summary>
        [JsonProperty("link")]
        public string Link { get; set; }
    }
}
