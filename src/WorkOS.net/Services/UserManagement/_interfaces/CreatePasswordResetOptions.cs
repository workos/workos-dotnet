namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to create a password reset token.
    /// </summary>
    public class CreatePasswordResetOptions : BaseOptions
    {
        /// <summary>
        /// The email address of the user requesting a password reset.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
