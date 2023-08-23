namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to create a password reset challenge.
    /// </summary>
    public class CreatePasswordResetChallengeOptions : ListOptions
    {
        /// <summary>
        /// The email of the user that wishes to reset their password.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// The URL that will be linked to in the email.
        /// </summary>
        [JsonProperty("password_reset_url")]
        public string PasswordResetUrl { get; set; }
    }
}
