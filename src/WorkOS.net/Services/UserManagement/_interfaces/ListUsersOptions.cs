namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to fetch Users.
    /// </summary>
    public class ListUsersOptions : ListOptions
    {
        /// <summary>
        /// Email of a <see cref="User"/>.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Organization ID to filter Users by.
        /// </summary>
        [JsonProperty("organization_id")]
        public string OrganizationId { get; set; }
    }
}
