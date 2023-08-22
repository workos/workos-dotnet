namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// An organization user has authorized access to.
    /// </summary>
    public class AuthorizedOrganization
    {
        /// <summary>
        /// Organization the user does have authorization to.
        /// </summary>
        [JsonProperty("organization")]
        public Organization Organization { get; set; }
    }
}
