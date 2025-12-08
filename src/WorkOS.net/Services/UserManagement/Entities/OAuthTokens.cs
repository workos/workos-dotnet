namespace WorkOS
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Contains OAuth tokens returned by an external OAuth provider.
    /// </summary>
    public class OAuthTokens
    {
        /// <summary>
        /// The access token from the OAuth provider.
        /// </summary>
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// The refresh token from the OAuth provider.
        /// </summary>
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// The timestamp at which the access token expires.
        /// </summary>
        [JsonProperty("expires_at")]
        public int ExpiresAt { get; set; }

        /// <summary>
        /// The list of OAuth scopes for which the access token is authorized.
        /// </summary>
        [JsonProperty("scopes")]
        public List<string> Scopes { get; set; }
    }
}
