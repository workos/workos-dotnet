namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// Contains information about a WorkOS Organization Domain record.
    /// </summary>
    public class OrganizationDomain
    {
        /// <summary>
        /// Description of the record.
        /// </summary>
        [JsonProperty("object")]
        public const string Object = "organization_domain";

        /// <summary>
        /// The Organization Domain's identifier.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The domain value.
        /// </summary>
        [JsonProperty("domain")]
        public string Domain { get; set; }

        /// <summary>
        /// The verification state of the domain.
        /// </summary>
        [JsonProperty("state")]
        public OrganizationDomainState? State { get; set; }

        /// <summary>
        /// How the domain needs to be verified.
        /// </summary>
        [JsonProperty("verification_strategy")]
        public OrganizationDomainVerificationStrategy? VerificationStrategy { get; set; }

        /// <summary>
        /// When the verification strategy is DNS, what the DNS record should be.
        /// </summary>
        [JsonProperty("verification_token")]
        public string VerificationToken { get; set; }
    }
}
