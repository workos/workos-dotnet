namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to verify user email using a verification token.
    /// </summary>
    public class CompleteEmailVerificationOptions : BaseOptions
    {
        /// <summary>
        /// The verification token emailed to the user.
        /// </summary>
        [JsonProperty("token")]
        public string Token { get; set; }
    }
}
