namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to authenticate a user with an authorization code.
    /// </summary>
    public class AuthenticateWithCodeOptions : BaseOptions
    {
        /// <summary>
        /// The authorization code received from the authorization server.
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }

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
        /// The code verifier used in PKCE flow (optional).
        /// </summary>
        [JsonProperty("code_verifier")]
        public string CodeVerifier { get; set; }

        /// <summary>
        /// The invitation token to accept an invitation during authentication (optional).
        /// </summary>
        [JsonProperty("invitation_token")]
        public string InvitationToken { get; set; }

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
        /// The grant type for authentication. Defaults to "authorization_code".
        /// </summary>
        [JsonProperty("grant_type")]
        public string GrantType { get; set; } = "authorization_code";
    }
}
