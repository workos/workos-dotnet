namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to authenticate a user with MagicAuth.
    /// </summary>
    public class AuthenticateUserWithMagicAuthOptions : BaseOptions
    {
        /// <summary>
        /// Specifies the grant type. Will always be `urn:workos:oauth:grant-type:magic-auth:code`.
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
        /// The ID of the User authenticating.
        /// </summary>
        [JsonProperty("user_id")]
        public string UserId { get; set; }

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
