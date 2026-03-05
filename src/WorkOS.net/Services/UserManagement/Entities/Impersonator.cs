namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// Contains information about a user who is impersonating another user.
    /// </summary>
    public class Impersonator
    {
        /// <summary>
        /// The email address of the impersonator.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// The reason for impersonation.
        /// </summary>
        [JsonProperty("reason")]
        public string Reason { get; set; }
    }
}
