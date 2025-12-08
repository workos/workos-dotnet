namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// Contains the response data from authenticating a user.
    /// </summary>
    public class AuthenticationResponse
    {
        /// <summary>
        /// The authenticated <see cref="User"/>.
        /// </summary>
        [JsonProperty("user")]
        public User User { get; set; }

        /// <summary>
        /// The organization ID associated with the user (optional).
        /// </summary>
        [JsonProperty("organization_id")]
        public string OrganizationId { get; set; }

        /// <summary>
        /// The access token for the authenticated session.
        /// </summary>
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// The refresh token for refreshing the access token.
        /// </summary>
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// The authentication method used.
        /// </summary>
        [JsonProperty("authentication_method")]
        public AuthenticationMethod AuthenticationMethod { get; set; }

        /// <summary>
        /// Information about the impersonator if the user was impersonated (optional).
        /// </summary>
        [JsonProperty("impersonator")]
        public Impersonator Impersonator { get; set; }

        /// <summary>
        /// OAuth tokens returned by the authentication provider (optional).
        /// </summary>
        [JsonProperty("oauth_tokens")]
        public OAuthTokens OAuthTokens { get; set; }
    }
}
