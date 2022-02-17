namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The response from the WorkOS API when enrolling a Factor.
    /// </summary>
    public class VerifyFactorResponse
    {
        /// <summary>
        /// Challenge Factor body response.
        /// </summary>
        [JsonProperty("challenge")]
        public Challenge Challenge { get; set; }

        /// <summary>
        /// Validity of code of challenge.
        /// </summary>
        [JsonProperty("valid")]
        public bool Valid { get; set; }
    }
}
