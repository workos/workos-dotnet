namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to fetch Directories.
    /// </summary>
    public class ListDirectoriesOptions : ListOptions
    {
        /// <summary>
        /// Searchable text for a <see cref="Directory"/>. Can be empty.
        /// </summary>
        [JsonProperty("search")]
        public string Search { get; set; }

        /// <summary>
        /// Organization ID of a <see cref="Directory"/>. Can be empty.
        /// </summary>
        [JsonProperty("organization_id")]
        public string OrganizationId { get; set; }
    }
}
