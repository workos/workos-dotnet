namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The response for SendVerificationEmail.
    /// </summary>
    public class SendVerificationEmailResponse
    {
        /// <summary>
        /// The corresponding User object.
        /// </summary>
        [JsonProperty("user")]
        public User User { get; set; }
    }
}
