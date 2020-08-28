namespace WorkOS
{
    using Newtonsoft.Json;

    public class Connection
    {
        [JsonProperty("object")]
        public const string Object = "connection";

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("connection_type")]
        public ConnectionType? ConnectionType { get; set; }

        [JsonProperty("oauth_uid")]
        public string OAuthUid { get; set; }

        [JsonProperty("oauth_secret")]
        public string OAuthSecret { get; set; }

        [JsonProperty("oauth_redirect_uri")]
        public string OAuthRedirectUri { get; set; }

        [JsonProperty("saml_entity_id")]
        public string SamlEntityId { get; set; }

        [JsonProperty("saml_idp_url")]
        public string SamlIdpUrl { get; set; }

        [JsonProperty("saml_relying_party_trust_cert")]
        public string SamlRelyingPartyTrustCert { get; set; }

        [JsonProperty("saml_x509_certs")]
        public string[] SamlX509Certs { get; set; }

        [JsonProperty("domains")]
        public ConnectionDomain[] Domains { get; set; }
    }
}
