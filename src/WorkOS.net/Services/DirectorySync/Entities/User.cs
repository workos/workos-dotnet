namespace WorkOS
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Contains information about a WorkOS Directory User record.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Description of the record.
        /// </summary>
        [JsonProperty("object")]
        public const string Object = "directory_user";

        /// <summary>
        /// The User's identifier.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The User's identity provider's identifier.
        /// </summary>
        [JsonProperty("idp_id")]
        public string IdpId { get; set; }

        /// <summary>
        /// The User's username.
        /// </summary>
        [JsonProperty("username")]
        public string Username { get; set; }

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
        /// The User's e-mails.
        /// </summary>
        [JsonProperty("emails")]
        public Email[] Emails { get; set; }

        /// <summary>
        /// The User's active state.
        /// </summary>
        [JsonProperty("state")]
        public UserState State { get; set; }

        /// <summary>
        /// The User's raw attributes.
        /// </summary>
        [JsonProperty("raw_attributes")]
        public Dictionary<string, object> RawAttributes { get; set; }

        /// <summary>
        /// Contains data about a User's e-mails.
        /// </summary>
        public class Email
        {
            /// <summary>
            /// Flag to indicate if the e-mail is primary.
            /// </summary>
            [JsonProperty("primary")]
            public bool Primary { get; set; }

            /// <summary>
            /// The User's e-mail.
            /// </summary>
            [JsonProperty("value")]
            public string Value { get; set; }

            /// <summary>
            /// The type of e-mail (ex. work).
            /// </summary>
            [JsonProperty("type")]
            public string Type { get; set; }
        }
    }
}
