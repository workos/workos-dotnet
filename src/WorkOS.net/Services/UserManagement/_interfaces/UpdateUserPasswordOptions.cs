namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to update the password of the user.
    /// </summary>
    public class UpdateUserPasswordOptions : BaseOptions
    {
        /// <summary>
        /// The ID of the user.
        /// </summary>
        [JsonIgnore]
        public string Id { get; set; }

        /// <summary>
        /// The new password for the user.
        /// </summary>
        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
