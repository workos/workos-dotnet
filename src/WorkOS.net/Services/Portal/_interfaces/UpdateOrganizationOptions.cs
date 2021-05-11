namespace WorkOS
{
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
        /// Domains of the <see cref="Organization"/>.
        /// </summary>
        [JsonProperty("domains")]
        public string[] Domains { get; set; }

        /// <summary>
        /// Name of the <see cref="Organization"/>.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
