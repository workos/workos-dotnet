namespace WorkOS
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Challenge record.
    /// </summary>
    public class Challenge
    {
        /// <summary>
        /// Description of the record.
        /// </summary>
        [JsonProperty("object")]
        public const string Object = "authentication_challenge";

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
        /// The timestamp of when the Factor will expire.
        /// </summary>
        [JsonProperty("expires_at")]
        public string ExpiresAt { get; set; }

        /// <summary>
        /// The verification code.
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }

        /// <summary>
        /// The factor id.
        /// </summary>
        [JsonProperty("authentication_factor_id")]
        public string FactorId { get; set; }
    }
}
