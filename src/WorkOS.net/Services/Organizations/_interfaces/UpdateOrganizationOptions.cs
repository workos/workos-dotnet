namespace WorkOS
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to update an Organization.
    /// </summary>
    public class UpdateOrganizationOptions : BaseOptions
    {
        /// <summary>
        /// Unique Identifier of the <see cref="Organization"/>.
        /// </summary>
        [JsonProperty("organization")]
        public string Organization { get; set; }

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
        /// Data for setting the domains of the <see cref="Organization"/>.
        /// </summary>
        [JsonProperty("domain_data")]
        public OrganizationDomainDataOptions[] DomainData { get; set; }

        /// <summary>
        /// Domains of the <see cref="Organization"/>.
        /// </summary>
        [ObsoleteAttribute("Use DomainData instead.", false)]
        [JsonProperty("domains")]
        public string[] Domains { get; set; }
    }
}
