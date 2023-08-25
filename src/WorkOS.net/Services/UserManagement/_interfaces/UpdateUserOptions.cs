namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to update the user.
    /// </summary>
    public class UpdateUserOptions : BaseOptions
    {
        /// <summary>
        /// The id of the user.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The email address of the user.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// The user's first name.
        /// </summary>
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        /// <summary>
        /// The user's last name.
        /// </summary>
        [JsonProperty("last_name")]
        public string LastName { get; set; }

        /// <summary>
        /// Whether the user's email address was previously verified.
        /// </summary>
        [JsonProperty("email_verified")]
        public bool IsEmailVerified { get; set; }
    }
}
