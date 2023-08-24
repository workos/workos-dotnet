namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to verify user email using a verification token.
    /// </summary>
    public class VerifyEmailOptions : BaseOptions
    {
        /// <summary>
        /// The challenge ID returned from the send verification email endpoint.
        /// </summary>
        [JsonProperty("magic_auth_challenge_id")]
        public string MagicAuthChallengeId { get; set; }

        /// <summary>
        /// The verification code emailed to the user.
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }
    }
}
