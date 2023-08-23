namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to complete a password reset challenge.
    /// </summary>
    public class CompletePasswordResetOptions : ListOptions
    {
        /// <summary>
        /// The reset token emailed to the user.
        /// </summary>
        [JsonProperty("token")]
        public string Token { get; set; }

        /// <summary>
        /// The new password to be set for the user.
        /// </summary>
        [JsonProperty("new_password")]
        public string NewPassword { get; set; }
    }
}
