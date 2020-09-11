namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The response from the WorkOS API when fetching a Profile.
    /// </summary>
    public class GetProfileResponse
    {
        /// <summary>
        /// An access token for the Profile.
        /// </summary>
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// An object containing information about an authenticated User.
        /// </summary>
        [JsonProperty("profile")]
        public Profile Profile { get; set; }
    }
}
