namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to create a User.
    /// </summary>
    public class CreateUserOptions : BaseOptions
    {
        /// <summary>
        /// The email address of the user.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// The password to set for the user.
        /// </summary>
        [JsonProperty("password")]
        public string Password { get; set; }

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
        public bool EmailVerified { get; set; }
    }
}
