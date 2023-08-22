namespace WorkOS
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Reasons for unauthorized access to an organization the user is a member of.
    /// </summary>
    public class MagicAuthChallenge
    {
        /// <summary>
        /// Description of the record.
        /// </summary>
        [JsonProperty("object")]
        public const string Object = "magic_auth_challenge";

        /// <summary>
        /// The Magic Auth Challenge ID.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
