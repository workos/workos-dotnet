namespace WorkOS
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to update a User.
    /// </summary>
    public class UpdateUserOptions : BaseOptions
    {
        /// <summary>
        /// Unique Identifier of the <see cref="User"/>.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

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
        /// Email of the <see cref="User"/>.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Whether the <see cref="User"/>'s email address has been verified.
        /// </summary>
        [JsonProperty("email_verified")]
        public bool? EmailVerified { get; set; }

        /// <summary>
        /// Password of the <see cref="User"/>.
        /// </summary>
        [JsonProperty("password")]
        public string Password { get; set; }

        /// <summary>
        /// Hash of the <see cref="User"/>'s password. Mutually exclusive with <see cref="Password"/>.
        /// </summary>
        [JsonProperty("password_hash")]
        public string PasswordHash { get; set; }

        /// <summary>
        /// Type of the <see cref="User"/>'s password hash.
        /// </summary>
        [JsonProperty("password_hash_type")]
        public UserPasswordHashType PasswordHashType { get; set; }

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

        /// <summary>
        /// Preferred locale for the <see cref="User"/>.
        /// </summary>
        [JsonProperty("locale")]
        public string Locale { get; set; }
    }
}
