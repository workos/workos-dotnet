namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a user's role.
    /// </summary>
    public class RoleResponse
    {
        /// <summary>
        /// The slug identifier for the role.
        /// </summary>
        [JsonProperty("slug")]
        public string Slug { get; set; }
    }
}