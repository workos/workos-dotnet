namespace WorkOS
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Contains information about an authenticated User.
    /// </summary>
    public class Profile
    {
        /// <summary>
        /// The identifier for the Profile.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The Profile's identity provider's identifier.
        /// </summary>
        [JsonProperty("idp_id")]
        public string IdpId { get; set; }

        /// <summary>
        /// The Connection type associated with the Profile.
        /// </summary>
        [JsonProperty("connection_type")]
        public ConnectionType? ConnectionType { get; set; }

        /// <summary>
        /// The User's e-mail.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// The first name of the User.
        /// </summary>
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        /// <summary>
        /// The last name of the User.
        /// </summary>
        [JsonProperty("last_name")]
        public string LastName { get; set; }

        /// <summary>
        /// The raw response of Profile attributes from the identity provider.
        /// </summary>
        [JsonProperty("raw_attributes")]
        public Dictionary<string, object> RawAttributes { get; set; }
    }
}
