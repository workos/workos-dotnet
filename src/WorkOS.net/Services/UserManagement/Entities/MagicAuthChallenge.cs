namespace WorkOS
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a request for a Magic Auth code.
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
