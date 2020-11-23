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
        /// The location that the user will be redirected to after authenticating.
        /// </summary>
        [JsonProperty("redirect_uri")]
        public string RedirectURI { get; set; }

        /// <summary>
        /// An optional parameter to encode information throughout the
        /// authentication life cycle.
        /// </summary>
        [JsonProperty("state")]
        public string State { get; set; }
    }
}
