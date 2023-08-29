namespace WorkOS
{
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a response for a Magic Auth code.
    /// </summary>
    public class SendMagicAuthCodeResponse
    {
        /// <summary>
        /// The corresponding User object.
        /// </summary>
        [JsonProperty("user")]
        public User User { get; set; }
    }
}