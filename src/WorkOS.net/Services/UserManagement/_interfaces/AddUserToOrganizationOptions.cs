namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameter to add an unmanaged user to an organization.
    /// </summary>
    public class AddUserToOrganizationOptions : BaseOptions
    {
        /// <summary>
        /// Unique identifier of the Organization.
        /// </summary>
        [JsonProperty("organization_id")]
        public string Organization { get; set; }
    }
}
