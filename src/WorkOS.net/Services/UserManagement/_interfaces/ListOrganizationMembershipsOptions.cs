namespace WorkOS
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to fetch organization memberships.
    /// </summary>
    public class ListOrganizationMembershipsOptions : ListOptions
    {
        /// <summary>
        /// User ID to filter memberships by.
        /// </summary>
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        /// <summary>
        /// Organization ID to filter memberships by.
        /// </summary>
        [JsonProperty("organization_id")]
        public string OrganizationId { get; set; }

        /// <summary>
        /// Filter by membership statuses.
        /// </summary>
        [JsonProperty("statuses")]
        public List<string> Statuses { get; set; }
    }
}
