namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// Contains information about a WorkOS Connection record.
    /// </summary>
    public class Connection
    {
        /// <summary>
        /// Description of the record.
        /// </summary>
        [JsonProperty("object")]
        public const string Object = "connection";

        /// <summary>
        /// The Connection's identifier.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The linked status of the Connection.
        /// </summary>
        [JsonProperty("status")]
        public ConnectionStatus Status { get; set; }

        /// <summary>
        /// The name of the Connection.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The type of the Connection.
        /// </summary>
        [JsonProperty("connection_type")]
        public ConnectionType? ConnectionType { get; set; }

        /// <summary>
        /// OAuth Client identifier.
        /// </summary>
        [JsonProperty("oauth_uid")]
        public string OAuthUid { get; set; }

        /// <summary>
        /// OAuth Client secret.
        /// </summary>
        [JsonProperty("oauth_secret")]
        public string OAuthSecret { get; set; }

        /// <summary>
        /// OAuth Redirect URI.
        /// </summary>
        [JsonProperty("oauth_redirect_uri")]
        public string OAuthRedirectUri { get; set; }

        /// <summary>
        /// Identity Provider Issuer.
        /// </summary>
        [JsonProperty("saml_entity_id")]
        public string SamlEntityId { get; set; }

        /// <summary>
        /// Identity Provider SSO URL.
        /// </summary>
        [JsonProperty("saml_idp_url")]
        public string SamlIdpUrl { get; set; }

        /// <summary>
        /// Certificate that describes where to expect valid SAML claims to
        /// come from.
        /// </summary>
        [JsonProperty("saml_relying_party_trust_cert")]
        public string SamlRelyingPartyTrustCert { get; set; }

        /// <summary>
        /// Certificates used to authenticate SAML assertions.
        /// </summary>
        [JsonProperty("saml_x509_certs")]
        public string[] SamlX509Certs { get; set; }

        /// <summary>
        /// Domain records for the Connection.
        /// </summary>
        [JsonProperty("domains")]
        public ConnectionDomain[] Domains { get; set; }
    }
}
