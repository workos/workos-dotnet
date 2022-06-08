namespace WorkOS
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Contains information about an MFA Factor record.
    /// </summary>
    public class Factor
    {
        /// <summary>
        /// Description of the record.
        /// </summary>
        [JsonProperty("object")]
        public const string Object = "authentication_factor";

        /// <summary>
        /// The Authentication Factor's identifier.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The timestamp of when the Factor was created.
        /// </summary>
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        /// <summary>
        /// The timestamp of when the Factor was updated.
        /// </summary>
        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }

        /// <summary>
        /// The name of the Connection.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Totp details when enroll response is Totp.
        /// </summary>
        [JsonProperty("totp")]
        public Totp Totp { get; set; }

        /// <summary>
        /// Sms details when enroll response is Sms.
        /// </summary>
        [JsonProperty("sms")]
        public Sms Sms { get; set; }
    }
}
