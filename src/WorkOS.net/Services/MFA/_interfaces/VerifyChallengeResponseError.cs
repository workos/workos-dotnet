namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The response from the WorkOS API when verifying an authentication challenge.
    /// </summary>
    public class VerifyChallengeResponseError : VerifyChallengeResponse
    {
        /// <summary>
        /// The error code.
        /// </summary>
        [JsonProperty("code")]
        public string ErrorCode { get; set; }

        /// <summary>
        /// The description of error.
        /// </summary>
        [JsonProperty("message")]
        public string ErrorMessage { get; set; }
    }
}
