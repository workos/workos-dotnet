namespace WorkOS
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to create a User.
    /// </summary>
    public class CreateUserOptions : BaseOptions
    {
        /// <summary>
        /// Email of the <see cref="User"/>.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Password for the <see cref="User"/>.
        /// </summary>
        [JsonProperty("password")]
        public string Password { get; set; }

        /// <summary>
        /// Password hash for the <see cref="User"/>.
        /// </summary>
        [JsonProperty("password_hash")]
        public string PasswordHash { get; set; }

        /// <summary>
        /// Password hash type for the <see cref="User"/>.
        /// </summary>
        [JsonProperty("password_hash_type")]
        public UserPasswordHashType PasswordHashType { get; set; }

        /// <summary>
        /// First name of the <see cref="User"/>.
        /// </summary>
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        /// <summary>
        /// Last name of the <see cref="User"/>.
        /// </summary>
        [JsonProperty("last_name")]
        public string LastName { get; set; }

        /// <summary>
        /// Whether the <see cref="User"/>'s email address has been verified.
        /// </summary>
        [JsonProperty("email_verified")]
        public bool? EmailVerified { get; set; }

        /// <summary>
        /// External identifier for the <see cref="User"/>.
        /// </summary>
        [JsonProperty("external_id")]
        public string ExternalId { get; set; }

        /// <summary>
        /// Metadata for the <see cref="User"/>.
        /// </summary>
        [JsonProperty("metadata")]
        public Dictionary<string, string> Metadata { get; set; }
    }
}
