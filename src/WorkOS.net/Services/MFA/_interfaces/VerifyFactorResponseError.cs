namespace WorkOS
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// The response from the WorkOS API when enrolling a Factor.
    /// </summary>
    [Obsolete("Please use VerifyChallengeResponseError instead.")]
    public class VerifyFactorResponseError : VerifyFactorResponse
    {
        /// <summary>
        /// Describe error code.
        /// </summary>
        [JsonProperty("code")]
        public string ErrorCode { get; set; }

        /// <summary>
        /// Description of error.
        /// </summary>
        [JsonProperty("message")]
        public string ErrorMessage { get; set; }
    }
}
