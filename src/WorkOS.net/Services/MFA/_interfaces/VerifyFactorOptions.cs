 namespace WorkOS
{
    using Newtonsoft.Json;

    public class VerifyFactorOptions : BaseOptions
    {
        /// <summary>
        /// Auth ID of the Challenge.
        /// </summary>
        [JsonProperty("authentication_challenge_id")]
        public string ChallengeId { get; set; }

        /// <summary>
        /// Describes which type to use.
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }
    }
}
