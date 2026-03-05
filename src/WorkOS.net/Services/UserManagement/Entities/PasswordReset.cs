namespace WorkOS
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Contains information about a WorkOS Password Reset record.
    /// </summary>
    public class PasswordReset
    {
        /// <summary>
        /// The Password Reset identifier.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The User identifier associated with this password reset.
        /// </summary>
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        /// <summary>
        /// The email address associated with this password reset.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// The password reset token that can be used to reset the user's password.
        /// </summary>
        [JsonProperty("password_reset_token")]
        public string PasswordResetToken { get; set; }

        /// <summary>
        /// The URL where the user can reset their password using the token.
        /// </summary>
        [JsonProperty("password_reset_url")]
        public string PasswordResetUrl { get; set; }

        /// <summary>
        /// The timestamp when this password reset token expires.
        /// </summary>
        [JsonProperty("expires_at")]
        public string ExpiresAt { get; set; }

        /// <summary>
        /// The timestamp of when the password reset was created.
        /// </summary>
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }
    }
}
