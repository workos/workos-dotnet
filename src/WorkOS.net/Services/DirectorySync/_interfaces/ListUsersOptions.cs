namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to fetch Directory Users.
    /// </summary>
    public class ListUsersOptions : ListOptions
    {
        /// <summary>
        /// Unique identifier of a <see cref="Directory"/>.
        /// </summary>
        [JsonProperty("directory")]
        public string Directory { get; set; }

        /// <summary>
        /// Unique identifier of a <see cref="Group"/>.
        /// </summary>
        [JsonProperty("group")]
        public string Group { get; set; }
    }
}
