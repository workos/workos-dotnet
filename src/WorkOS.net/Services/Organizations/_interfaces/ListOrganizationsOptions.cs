namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to fetch Organizations.
    /// </summary>
    public class ListOrganizationsOptions : ListOptions
    {
        /// <summary>
        /// Domains of an <see cref="Organization"/>. Can be empty.
        /// </summary>
        [JsonProperty("domains")]
        public string[] Domains { get; set; }
    }
}
