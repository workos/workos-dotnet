namespace WorkOS
{
    using System;
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
        public string ClientId { get; set; }

        /// <summary>
        /// The Enterprise's domain.
        /// </summary>
        [Obsolete("The Domain property is deprecated. Please use Organization instead.", error: false)]
        [JsonProperty("domain")]
        public string Domain { get; set; }

        /// <summary>
        /// Domain hint that will be passed as a parameter to the IdP login page.
        /// </summary>
        [JsonProperty("domain_hint")]
        public string DomainHint { get; set; }

        /// <summary>
        /// Username/email hint that will be passed as a parameter to the to IdP login page.
        /// </summary>
        [JsonProperty("login_hint")]
        public string LoginHint { get; set; }

        /// <summary>
        /// The unique identifier for a <see cref="Connection"/> record.
        /// </summary>
        [JsonProperty("connection")]
        public string Connection { get; set; }

        /// <summary>
        /// The unique identifier for an <see cref="Organization"/> record.
        /// </summary>
        [JsonProperty("organization")]
        public string Organization { get; set; }

        /// <summary>
        /// An optional parameter that specifies the type of Connection to
        /// authenticate with. If used, a user of any domain can be
        /// authenticated. Only `GoogleOAuth` is currently supported.
        /// </summary>
        [JsonProperty("provider")]
        public ConnectionType? Provider { get; set; }

        /// <summary>
        /// A callback URL where the application redirects the user-agent after
        /// an authorization code is granted.
        /// </summary>
        [JsonProperty("redirect_uri")]
        public string RedirectURI { get; set; }

        /// <summary>
        /// An optional parameter to manage state across the authorization
        /// lifecycle.
        /// </summary>
        [JsonProperty("state")]
        public string State { get; set; }
    }
}
