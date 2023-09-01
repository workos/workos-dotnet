namespace WorkOS
{
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a response for a Verify Email.
    /// </summary>
    public class VerifyEmailResponse
    {
        /// <summary>
        /// The corresponding User object.
        /// </summary>
        [JsonProperty("user")]
        public User User { get; set; }
    }
}
