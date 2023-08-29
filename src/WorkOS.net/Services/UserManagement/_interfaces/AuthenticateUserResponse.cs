namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to authenticate a user with a password.
    /// </summary>
    public class AuthenticateUserResponse
    {
        /// <summary>
        /// Represents the corresponding User object.
        /// </summary>
        [JsonProperty("user")]
        public User User { get; set; }
    }
}
