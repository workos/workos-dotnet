namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to create an Organization.
    /// </summary>
    public class CreateOrganizationOptions : BaseOptions
    {
        /// <summary>
        /// Name of the <see cref="Organization"/>.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Whether Connections within the Organization allow profiles that are
        /// outside of the Organization's configured User Email Domains.
        /// </summary>
        [JsonProperty("allow_profiles_outside_organization")]
        public bool? AllowProfilesOutsideOrganization { get; set; }

        /// <summary>
        /// Domains of the <see cref="Organization"/>.
        /// </summary>
        [JsonProperty("domains")]
        public string[] Domains { get; set; }
    }
}
