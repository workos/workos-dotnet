namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to generate a link to the WorkOS Admin Portal.
    /// </summary>
    public class GenerateLinkOptions : BaseOptions
    {
        /// <summary>
        /// Intent of the Admin Portal.
        /// </summary>
        [JsonProperty("intent")]
        public Intent Intent { get; set; }

        /// <summary>
        /// <see cref="Organization"/> identifier to scope the Portal session.
        /// </summary>
        [JsonProperty("organization")]
        public string Organization { get; set; }

        /// <summary>
        /// The URL to which WorkOS should send users when they click on the link to return to your website.
        /// </summary>
        [JsonProperty("return_url")]
        public string ReturnURL { get; set; }

        /// <summary>
        /// The URL to which WorkOS will redirect users to upon successfully setting up Single Sign On or Directory Sync.
        /// </summary>
        [JsonProperty("success_url")]
        public string SuccessURL { get; set; }
    }
}
