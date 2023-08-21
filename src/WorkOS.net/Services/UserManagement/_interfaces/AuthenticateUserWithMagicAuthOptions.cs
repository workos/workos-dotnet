namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to authenticate a user with a token.
    /// </summary>
    public class AuthenticateUserWithMagicAuthOptions : BaseOptions
    {
        /// <summary>
        /// Specifies the grant type. Will always be `authorization_code`.
        /// </summary>
        [JsonProperty("grant_type")]
        public const string GrantType = "urn:workos:oauth:grant-type:magic-auth:code";

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
        /// The authorization value which was passed back as a query parameter in the callback to the Redirect URI.
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }

        /// <summary>
        /// The challenge ID returned from when the one-time code was sent to the user.
        /// </summary>
        [JsonProperty("magic_auth_challenge_id")]
        public string MagicAuthChallengeId { get; set; }

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

        /// <summary>
        /// The length of the session in minutes. Defaults to 1 day, 1440.
        /// </summary>
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
