namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The response from the WorkOS API when enrolling a Factor.
    /// </summary>
    public class ChallengeFactorResponse
    {
        /// <summary>
        /// Object type
        /// </summary>
        [JsonProperty("object")]
        public string Object { get; set; }

        /// <summary>
        /// Unique factor identifier
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Created_at date
        /// </summary>
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        /// <summary>
        /// Updated_at date
        /// </summary>
        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }

        /// <summary>
        /// Type of MFA
        /// </summary>
        [JsonProperty("expires_at")]
        public string ExpiresAt { get; set; }

        /// <summary>
        /// Environment ID
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }

        /// <summary>
        /// Environment ID
        /// </summary>
        [JsonProperty("authentication_id")]
        public string AuthenticationId { get; set; }
    }
}
