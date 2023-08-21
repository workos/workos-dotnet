namespace WorkOS
{
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Reasons for unauthorized access to an organization the user is a member of.
    /// </summary>
    public class UnauthorizedOrganization
    {
        /// <summary>
        /// Organization the user does not have authorization to.
        /// </summary>
        [JsonProperty("organization")]
        public Organization Organization { get; set; }

        /// <summary>
        /// Reason user does not have authorization to organization.
        /// </summary>
        [JsonProperty("reasons")]
        public List<Reason> Reasons { get; set; }
    }
}
