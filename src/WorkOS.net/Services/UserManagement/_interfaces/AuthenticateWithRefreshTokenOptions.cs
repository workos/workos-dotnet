namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to authenticate a user with a refresh token.
    /// </summary>
    public class AuthenticateWithRefreshTokenOptions : BaseOptions
    {
        /// <summary>
        /// The refresh token to exchange for a new access token.
        /// </summary>
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// The client ID for your WorkOS environment.
        /// </summary>
        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        /// <summary>
        /// The client secret for your WorkOS environment.
        /// </summary>
        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; }

        /// <summary>
        /// The organization ID to switch to for this refresh (optional).
        /// </summary>
        [JsonProperty("organization_id")]
        public string OrganizationId { get; set; }

        /// <summary>
        /// The IP address of the user authenticating (optional).
        /// </summary>
        [JsonProperty("ip_address")]
        public string IpAddress { get; set; }

        /// <summary>
        /// The user agent string of the authenticating user (optional).
        /// </summary>
        [JsonProperty("user_agent")]
        public string UserAgent { get; set; }

        /// <summary>
        /// The grant type for token refresh. Defaults to "refresh_token".
        /// </summary>
        [JsonProperty("grant_type")]
        public string GrantType { get; set; } = "refresh_token";
    }
}
