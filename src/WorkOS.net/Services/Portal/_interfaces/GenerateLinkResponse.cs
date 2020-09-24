namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The response from generating an Admin Portal link.
    /// </summary>
    public class GenerateLinkResponse
    {
        /// <summary>
        /// The generated link to the Admin Portal.
        /// </summary>
        [JsonProperty("link")]
        public string Link { get; set; }
    }
}
