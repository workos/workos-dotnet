namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// Contains information about a role in an organization membership.
    /// </summary>
    public class OrganizationMembershipRole
    {
        /// <summary>
        /// The slug identifier of the role.
        /// </summary>
        [JsonProperty("slug")]
        public string Slug { get; set; }
    }
}
