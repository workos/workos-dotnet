namespace WorkOS
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Contains information about a WorkOS Organization Membership record.
    /// </summary>
    public class OrganizationMembership
    {
        /// <summary>
        /// Description of the record.
        /// </summary>
        [JsonProperty("object")]
        public const string Object = "organization_membership";

        /// <summary>
        /// The organization membership's identifier.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

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
        /// The organization's name.
        /// </summary>
        [JsonProperty("organization_name")]
        public string OrganizationName { get; set; }

        /// <summary>
        /// The primary role assigned to the membership.
        /// </summary>
        [JsonProperty("role")]
        public OrganizationMembershipRole Role { get; set; }

        /// <summary>
        /// List of roles assigned to the membership.
        /// </summary>
        [JsonProperty("roles")]
        public List<OrganizationMembershipRole> Roles { get; set; }

        /// <summary>
        /// The status of the organization membership.
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }

        /// <summary>
        /// The timestamp of when the organization membership was created.
        /// </summary>
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        /// <summary>
        /// The timestamp of when the organization membership was updated.
        /// </summary>
        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }
    }
}
