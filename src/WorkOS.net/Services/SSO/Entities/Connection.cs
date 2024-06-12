namespace WorkOS
{
    using System;
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
        /// The unique identifier for the Organization in which the Connection resides.
        /// </summary>
        [JsonProperty("organization_id")]
        public string OrganizationId { get; set; }

        /// <summary>
        /// The name of the Connection.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The type of the Connection.
        /// One of:
        /// <c>ADFSSAML</c>
        /// <c>AdpOidc</c>
        /// <c>AppleOAuth</c>
        /// <c>Auth0SAML</c>
        /// <c>AzureSAML</c>
        /// <c>CasSAML</c>
        /// <c>CloudflareSAML</c>
        /// <c>ClasslinkSAML</c>
        /// <c>CyberArkSAML</c>
        /// <c>DuoSAML</c>
        /// <c>GenericOIDC</c>
        /// <c>GenericSAML</c>
        /// <c>GitHubOAuth</c>
        /// <c>GoogleOAuth</c>
        /// <c>GoogleSAML</c>
        /// <c>JumpCloudSAML</c>
        /// <c>KeycloakSAML</c>
        /// <c>LastPassSAML</c>
        /// <c>MagicLink</c>
        /// <c>MicrosoftOAuth</c>
        /// <c>MiniOrangeSAML</c>
        /// <c>NetIqSAML</c>
        /// <c>OktaSAML</c>
        /// <c>OneLoginSAML</c>
        /// <c>OracleSAML</c>
        /// <c>PingFederateSAML</c>
        /// <c>PingOneSAML</c>
        /// <c>RipplingSAML</c>
        /// <c>SalesforceSAML</c>
        /// <c>ShibbolethGenericSAML</c>
        /// <c>ShibbolethSAML</c>
        /// <c>SimpleSamlPhpSAML</c>
        /// <c>VMwareSAML</c>.
        /// </summary>
        [JsonProperty("connection_type")]
        public string Type { get; set; }

        /// <summary>
        /// The linked state of the Connection.
        /// </summary>
        [JsonProperty("state")]
        public ConnectionState State { get; set; }

        /// <summary>
        /// Domain records for the Connection.
        /// </summary>
        [JsonProperty("domains")]
        public ConnectionDomain[] Domains { get; set; }

        /// <summary>
        /// The timestamp of when the Connection was created.
        /// </summary>
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        /// <summary>
        /// The timestamp of when the Connection was updated.
        /// </summary>
        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }
    }
}
