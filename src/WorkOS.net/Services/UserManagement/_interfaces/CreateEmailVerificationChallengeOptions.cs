namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to create an email verification challenge.
    /// </summary>
    public class CreateEmailVerificationChallengeOptions : BaseOptions
    {
        /// <summary>
        /// The URL that will be linked to in the verification email.
        /// </summary>
        [JsonProperty("verification_url")]
        public string VerificationUrl { get; set; }
    }
}
