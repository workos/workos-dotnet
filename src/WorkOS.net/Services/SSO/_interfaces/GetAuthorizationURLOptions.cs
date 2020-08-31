namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// Describes options to create a WorkOS Authorization URL.
    /// </summary>
    public class GetAuthorizationURLOptions : BaseOptions
    {
        /// <summary>
        /// Describes which grant to use. Will always be `code`.
        /// </summary>
        [JsonProperty("response_type")]
        public const string ResponseType = "code";

        /// <summary>
        /// The WorkOS Project identifier. 
        /// </summary>
        [JsonProperty("client_id")]
        public string ClientId;

        /// <summary>
        /// The Enterprise's domain.
        /// </summary>
        [JsonProperty("domain")]
        public string Domain;

        /// <summary>
        /// An optional parameter that specifies the type of Connection to
        /// authenticate with. If used, a user of any domain can be
        /// authenticated. Only `GoogleOAuth` is currently supported.
        /// </summary>
        [JsonProperty("provider")]
        public ConnectionType? Provider;

        /// <summary>
        /// A callback URL where the application redirects the user-agent after
        /// an authorization code is granted.
        /// </summary>
        [JsonProperty("redirect_uri")]
        public string RedirectURI;

        /// <summary>
        /// An optional parameter to manage state across the authorization
        /// lifecycle.
        /// </summary>
        [JsonProperty("state")]
        public string State;
    }
}
