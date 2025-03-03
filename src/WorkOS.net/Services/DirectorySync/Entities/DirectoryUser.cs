namespace WorkOS
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Contains information about a WorkOS Directory User record.
    /// </summary>
    public class DirectoryUser
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
        /// The identifier of the Directory the Directory User belongs to.
        /// </summary>
        [JsonProperty("directory_id")]
        public string DirectoryId { get; set; }

        /// <summary>
        /// The identifier for the Organization in which the Directory resides.
        /// </summary>
        [JsonProperty("organization_id")]
        public string OrganizationId { get; set; }

        /// <summary>
        /// The User's directory provider's identifier.
        /// </summary>
        [JsonProperty("idp_id")]
        public string IdpId { get; set; }

        /// <summary>
        /// The User's username.
        /// </summary>
        [ObsoleteAttribute("Will be removed in a future major version. Enable the `username` custom attribute in dashboard and pull from customAttributes instead. See https://workos.com/docs/directory-sync/attributes/custom-attributes/auto-mapped-attributes for details.", true)]
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
        /// The User's job title.
        /// </summary>
        [ObsoleteAttribute("Will be removed in a future major version. Enable the `job_title` custom attribute in dashboard and pull from customAttributes instead. See https://workos.com/docs/directory-sync/attributes/custom-attributes/auto-mapped-attributes for details.", true)]
        [JsonProperty("job_title")]
        public string JobTitle { get; set; }

        /// <summary>
        /// The primary email of the Directory User.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// The User's e-mails.
        /// </summary>
        [ObsoleteAttribute("Will be removed in a future major version. Enable the `emails` custom attribute in dashboard and pull from customAttributes instead. See https://workos.com/docs/directory-sync/attributes/custom-attributes/auto-mapped-attributes for details.", true)]
        [JsonProperty("emails")]
        public EmailObject[] Emails { get; set; }

        /// <summary>
        /// The User's groups.
        /// </summary>
        [JsonProperty("groups")]
        public List<Group> Groups { get; set; }

        /// <summary>
        /// The User's active state.
        /// </summary>
        [JsonProperty("state")]
        public DirectoryUserState State { get; set; }

        /// <summary>
        /// The timestamp of when the Directory User was created.
        /// </summary>
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        /// <summary>
        /// The timestamp of when the Directory User was updated.
        /// </summary>
        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }

        /// <summary>
        /// The User's raw attributes.
        /// </summary>
        [JsonProperty("raw_attributes")]
        public Dictionary<string, object> RawAttributes { get; set; }

        /// <summary>
        /// The User's custom attributes.
        /// </summary>
        [JsonProperty("custom_attributes")]
        public Dictionary<string, object> CustomAttributes { get; set; }

        /// <summary>
        /// The user's primary email.
        /// </summary>
        [ObsoleteAttribute("Use the `email` attribute instead.", true)]
        public EmailObject PrimaryEmail
        {
            get { return this.Emails.First(email => email.Primary == true); }
        }

        /// <summary>
        /// Contains data about a User's e-mails.
        /// </summary>
        public class EmailObject
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

        /// <summary>
        /// Contains data about a User's groups.
        /// </summary>
        public class Group
        {
            /// <summary>
            /// Description of the record.
            /// </summary>
            [JsonProperty("object")]
            public const string Object = "directory_group";

            /// <summary>
            /// The Group's identifier.
            /// </summary>
            [JsonProperty("id")]
            public string Id { get; set; }

            /// <summary>
            /// The Group's name.
            /// </summary>
            [JsonProperty("name")]
            public string Name { get; set; }
        }
    }
}
