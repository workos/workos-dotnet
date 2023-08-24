namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to verify user email using a verification token.
    /// </summary>
    public class VerifyEmailCodeOptions : BaseOptions
    {
        /// <summary>
        /// The verification code emailed to the user.
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }
    }
}
