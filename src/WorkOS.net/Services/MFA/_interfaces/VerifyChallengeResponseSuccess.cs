namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The response from the WorkOS API when verifying an authentication challenge.
    /// </summary>
    public class VerifyChallengeResponseSuccess : VerifyChallengeResponse
    {
        /// <summary>
        /// The verified authentication challenge.
        /// </summary>
        [JsonProperty("challenge")]
        public Challenge Challenge { get; set; }

        /// <summary>
        /// Whether the challenge is valid.
        /// </summary>
        [JsonProperty("valid")]
        public bool Valid { get; set; }
    }
}
