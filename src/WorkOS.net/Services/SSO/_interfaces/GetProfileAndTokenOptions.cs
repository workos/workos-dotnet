namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// Describes the options to get a Profile.
    /// </summary>
    public class GetProfileAndTokenOptions : BaseOptions
    {
        /// <summary>
        /// Specifies the grant type. Will always be `authorization_code`.
        /// </summary>
        [JsonProperty("grant_type")]
        public const string GrantType = "authorization_code";

        /// <summary>
        /// The WorkOS Project identifier.
        /// </summary>
        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        /// <summary>
        /// The WorkOS Project secret key. This value is provided by the
        /// <see cref="WorkOSClient"/>.
        /// </summary>
        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; }

        /// <summary>
        /// An authorization code used to exchange for a Profile.
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }
    }
}
