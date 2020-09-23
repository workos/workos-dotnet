namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to fetch Directory Groups.
    /// </summary>
    public class ListGroupsOptions : ListOptions
    {
        /// <summary>
        /// Unique identifier of a <see cref="Directory"/>.
        /// </summary>
        [JsonProperty("directory")]
        public string Directory { get; set; }

        /// <summary>
        /// Unique identifier of a <see cref="User"/>.
        /// </summary>
        [JsonProperty("user")]
        public string User { get; set; }
    }
}
