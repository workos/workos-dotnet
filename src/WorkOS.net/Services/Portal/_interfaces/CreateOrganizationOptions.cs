namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to create an Organization.
    /// </summary>
    public class CreateOrganizationOptions : BaseOptions
    {
        /// <summary>
        /// Domains of the <see cref="Organization"/>.
        /// </summary>
        [JsonProperty("domains")]
        public string[] Domains { get; set; }

        /// <summary>
        /// Name of the <see cref="Organization"/>.
        /// </summary>
        public string Name { get; set; }
    }
}
