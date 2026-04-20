// @oagen-ignore-file
namespace WorkOS
{
    using Newtonsoft.Json;
    using STJS = System.Text.Json.Serialization;

    /// <summary>
    /// Contains pagination options for WorkOS records.
    /// </summary>
    public class ListMetadata
    {
        /// <summary>
        /// Cursor to receive records before a provided identifier.
        /// </summary>
        [JsonProperty("before")]
        [STJS.JsonPropertyName("before")]
        public string? Before { get; set; }

        /// <summary>
        /// Cursor to receive records after a provided identifier.
        /// </summary>
        [JsonProperty("after")]
        [STJS.JsonPropertyName("after")]
        public string? After { get; set; }
    }
}
