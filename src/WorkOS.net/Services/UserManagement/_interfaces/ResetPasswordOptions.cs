namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to reset a user's password.
    /// </summary>
    public class ResetPasswordOptions : BaseOptions
    {
        /// <summary>
        /// The password reset token from the password reset request.
        /// </summary>
        [JsonProperty("token")]
        public string Token { get; set; }

        /// <summary>
        /// The new password for the user.
        /// </summary>
        [JsonProperty("new_password")]
        public string NewPassword { get; set; }
    }
}
