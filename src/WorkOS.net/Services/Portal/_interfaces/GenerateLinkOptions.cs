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
        /// The URL users will return to when finished with the Admin Portal.
        /// </summary>
        [JsonProperty("return_url")]
        public string ReturnURL { get; set; }
    }
}
