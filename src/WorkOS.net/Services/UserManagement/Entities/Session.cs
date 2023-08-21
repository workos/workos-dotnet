namespace WorkOS
{
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Contains information about a user session.
    /// </summary>
    public class Session
    {
        /// <summary>
        /// The Session ID.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The timestamp of when the session was created.
        /// </summary>
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        /// <summary>
        /// The timestamp of when the session will expire.
        /// </summary>
        [JsonProperty("expires_at")]
        public string ExpiresAt { get; set; }

        /// <summary>
        /// The token which can be used to verify the user's session.
        /// </summary>
        [JsonProperty("token")]
        public string Token { get; set; }

        /// <summary>
        /// List of organizations with authorized access that the user is a member of.
        /// </summary>
        [JsonProperty("authorized_organizaitons")]
        public List<AuthorizedOrganizations> AuthorizedOrganizations { get; set; }

        /// <summary>
        /// List of organizations with unauthorized access that the user is a member of.
        /// </summary>
        [JsonProperty("unauthorized_organizations")]
        public List<UnauthorizedOrganizations> UnauthorizedOrganizations { get; set; }
    }
}
