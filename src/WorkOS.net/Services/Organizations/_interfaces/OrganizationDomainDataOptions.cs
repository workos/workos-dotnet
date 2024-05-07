namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to configure a Domain as part of an Organization.
    /// </summary>
    public class OrganizationDomainDataOptions : BaseOptions
    {
        /// <summary>
        /// Domain of the <see cref="Organization"/>.
        /// </summary>
        [JsonProperty("domain")]
        public string Domain { get; set; }

        /// <summary>
        /// State of the <see cref="OrganizationDomainDataOptions"/>.
        /// </summary>
        [JsonProperty("state")]
        public OrganizationDomainDataState State { get; set; }
    }
}
