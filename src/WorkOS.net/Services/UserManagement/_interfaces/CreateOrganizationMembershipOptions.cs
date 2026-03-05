namespace WorkOS
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to create an organization membership.
    /// </summary>
    public class CreateOrganizationMembershipOptions : BaseOptions
    {
        /// <summary>
        /// The user's identifier.
        /// </summary>
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        /// <summary>
        /// The organization's identifier.
        /// </summary>
        [JsonProperty("organization_id")]
        public string OrganizationId { get; set; }

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
