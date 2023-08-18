namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// Organizations user has authorized access to.
    /// </summary>
    public class AuthorizedOrganizations
    {
        /// <summary>
        /// Organization user does have authorization to.
        /// </summary>
        [JsonProperty("organization")]
        public Organization Organization { get; set; }
    }
}
