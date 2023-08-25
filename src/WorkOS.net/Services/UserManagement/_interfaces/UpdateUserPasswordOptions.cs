namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to update the password of the user.
    /// </summary>
    public class UpdateUserPasswordOptions : BaseOptions
    {
        /// <summary>
        /// The id of the user.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The email address of the user.
        /// </summary>
        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
