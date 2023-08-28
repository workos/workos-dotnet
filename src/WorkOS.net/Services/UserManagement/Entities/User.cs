namespace WorkOS
{
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Contains information about a User record.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Description of the record.
        /// </summary>
        [JsonProperty("object")]
        public const string Object = "user";

        /// <summary>
        /// The User's identifier.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The email of the user. For unmanaged users, their email is unique per authentication type in each WorkOS Environment. For managed users, emails are not unique.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// The User's first name.
        /// </summary>
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        /// <summary>
        /// The User's last name.
        /// </summary>
        [JsonProperty("last_name")]
        public string LastName { get; set; }

        /// <summary>
        /// The timestamp when the user's email was verified. Email verification is only applicable to unmanaged users.
        /// </summary>
        [JsonProperty("email_verified_at")]
        public string EmailVerifiedAt { get; set; }

        /// <summary>
        /// The ID of the Google OAuth Profile. Only unmanaged users who sign in with Google OAuth have Google OAuth Profiles.
        /// </summary>
        [JsonProperty("google_oauth_profile_id")]
        public string GoogleOauthProfileId { get; set; }

        /// <summary>
        /// The ID of the SSO Profile. Only managed users have SSO Profiles.
        /// </summary>
        [JsonProperty("sso_profile_id")]
        public string SsoProfileId { get; set; }

        /// <summary>
        /// The timestamp of when the User was created.
        /// </summary>
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        /// <summary>
        /// The timestamp of when the User was updated.
        /// </summary>
        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }
    }
}
