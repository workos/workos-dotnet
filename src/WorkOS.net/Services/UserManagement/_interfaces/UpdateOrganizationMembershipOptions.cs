namespace WorkOS
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to update an organization membership.
    /// </summary>
    public class UpdateOrganizationMembershipOptions : BaseOptions
    {
        /// <summary>
        /// The organization membership's identifier.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The slug identifier of the role to assign to the membership.
        /// </summary>
        [JsonProperty("role_slug")]
        public string RoleSlug { get; set; }

        /// <summary>
        /// The slug identifiers of roles to assign to the membership.
        /// </summary>
        [JsonProperty("role_slugs")]
        public List<string> RoleSlugs { get; set; }
    }
}
