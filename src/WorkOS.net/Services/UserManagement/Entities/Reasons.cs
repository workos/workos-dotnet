namespace WorkOS
{
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Reasons for unauthorized access to an organization the user is a member of.
    /// </summary>
    public class Reasons
    {
        /// <summary>
        /// The type of reason for unauthorized access to an organization.
        /// </summary>
        [JsonProperty("type")]
        public SessionUnauthorizedOrganizationReason Type { get; set; }

        /// <summary>
        /// Array of allowed authentication methods allowed by the organziation.
        /// </summary>
        [JsonProperty("allowed_authentication_methods")]
        public List<AuthenticationMethods> AllowedAuthenticationMethods { get; set; }
    }
}
