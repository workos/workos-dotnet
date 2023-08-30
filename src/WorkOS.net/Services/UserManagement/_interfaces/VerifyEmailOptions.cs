namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to verify user email using a verification token.
    /// </summary>
    public class VerifyEmailOptions : BaseOptions
    {
        /// <summary>
        /// The verification code emailed to the user.
        /// </summary>
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        /// <summary>
        /// The verification code emailed to the user.
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }
    }
}
