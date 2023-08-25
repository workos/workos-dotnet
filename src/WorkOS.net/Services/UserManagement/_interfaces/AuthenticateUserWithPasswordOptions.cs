namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to authenticate a user with a password.
    /// </summary>
    public class AuthenticateUserWithPasswordOptions : BaseOptions
    {
        /// <summary>
        /// Specifies the grant type. Will always be `authorization_code`.
        /// </summary>
        [JsonProperty("grant_type")]
        public const string GrantType = "password";

        /// <summary>
        /// This value can be obtained from the Configuration page in the WorkOS dashboard.
        /// </summary>
        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        /// <summary>
        /// This is the same value as the WorkOS Environment’s secret API key.
        /// </summary>
        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; }

        /// <summary>
        /// The email address of the user.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// The password of the user.
        /// </summary>
        [JsonProperty("password")]
        public string Password { get; set; }

        /// <summary>
        /// The IP address of the request from the user who is attempting to authenticate.
        /// </summary>
        [JsonProperty("ip_address")]
        public string IpAddress { get; set; }

        /// <summary>
        /// The user agent of the request from the user who is attempting to authenticate. This should be the value of the User-Agent header.
        /// </summary>
        [JsonProperty("user_agent")]
        public string UserAgent { get; set; }
    }
}
