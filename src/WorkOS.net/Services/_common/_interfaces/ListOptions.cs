// @oagen-ignore-file
namespace WorkOS
{
    using Newtonsoft.Json;
    using STJS = System.Text.Json.Serialization;

    /// <summary>
    /// Arguments when fetching a List of WorkOS records.
    /// </summary>
    public class ListOptions : BaseOptions
    {
        /// <summary>
        /// Pagination cursor to receive records before a provided identifier.
        /// </summary>
        [JsonProperty("before")]
        [STJS.JsonPropertyName("before")]
        public string? Before { get; set; }

        /// <summary>
        /// Pagination cursor to receive records after a provided identifier.
        /// </summary>
        [JsonProperty("after")]
        [STJS.JsonPropertyName("after")]
        public string? After { get; set; }

        /// <summary>
        /// Maximum number of records to return.
        /// </summary>
        [JsonProperty("limit")]
        [STJS.JsonPropertyName("limit")]
        public long? Limit { get; set; }

        /// <summary>
        /// The order in which to paginate records.
        /// </summary>
        [JsonProperty("order")]
        [STJS.JsonPropertyName("order")]
        public PaginationOrder Order { get; set; } = PaginationOrder.Desc;
    }
}
