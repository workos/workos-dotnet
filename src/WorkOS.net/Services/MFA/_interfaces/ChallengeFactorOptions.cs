 namespace WorkOS
{
    using Newtonsoft.Json;

    public class ChallengeFactorOptions : BaseOptions
    {
        /// <summary>
        /// Authentication Factor ID.
        /// </summary>
        [JsonProperty("authentication_factor_id")]
        public string FactorId { get; set; }
    }
}
