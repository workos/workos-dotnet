namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to create a Passwordless Session.
    /// </summary>
    public class CreatePasswordlessSessionOptions : BaseOptions
    {
        /// <summary>
        /// The e-mail of the user to authenticate.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// The type of Passwordless Session to create.
        /// </summary>
        [JsonProperty("type")]
        public PasswordlessSessionType Type { get; set; }

        /// <summary>
        /// An optional parameter of the location that the user will be
        /// redirected to after authenticating.
        /// </summary>
        [JsonProperty("redirect_uri")]
        public string RedirectURI { get; set; }

        /// <summary>
        /// An optional parameter of the ID of a specific connection. This can
        /// be used to create a Passwordless Session for a specific connection
        /// rather than using the domain from the email to determine the
        /// Organization and Connection.
        /// </summary>
        [JsonProperty("connection")]
        public string Connection { get; set; }

        /// <summary>
        /// An optional parameter. The number of seconds the Passwordless
        /// Session should live before expiring.
        /// This value must be between 300 (5 minutes) and 1800 (30 minutes),
        /// inclusive.
        /// </summary>
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        /// <summary>
        /// An optional parameter to encode information throughout the
        /// authentication life cycle.
        /// </summary>
        [JsonProperty("state")]
        public string State { get; set; }
    }
}
