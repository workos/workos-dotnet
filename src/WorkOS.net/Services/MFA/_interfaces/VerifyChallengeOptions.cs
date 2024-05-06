namespace WorkOS
{
    using Newtonsoft.Json;

    public class VerifyChallengeOptions : BaseOptions
    {
        /// <summary>
        /// The ID of the authentication challenge to verify.
        /// </summary>
        [JsonIgnore]
        public string ChallengeId { get; set; }

        /// <summary>
        /// The MFA code to verify.
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }
    }
}
