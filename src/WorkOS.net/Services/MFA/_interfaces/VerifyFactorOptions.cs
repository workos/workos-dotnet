 namespace WorkOS
{
    using Newtonsoft.Json;

    public class VerifyFactorOptions : BaseOptions
    {
        /// <summary>
        /// Describes which type to use.
        /// </summary>
        [JsonProperty("authentication_challenge_id")]
        public string Id { get; set; }

        /// <summary>
        /// Describes which type to use.
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }
    }
}
