namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to fetch Directory Users.
    /// </summary>
    public class ListUsersOptions : ListOptions
    {
        /// <summary>
        /// Filter Users by their type.
        /// </summary>
        [JsonProperty("type")]
        public UserType Type { get; set; }

        /// <summary>
        /// Filter Users by their email.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Filter Users by the organization they are members of.
        /// </summary>
        [JsonProperty("organization")]
        public string Organization { get; set; }
    }
}
