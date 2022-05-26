namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// Arguments when fetching a List of WorkOS records.
    /// </summary>
    public class ListOptions : BaseOptions
    {
        /// <summary>
        /// Pagination cursor to receive records before a provided identifier.
        /// </summary>
        [JsonProperty("before")]
        public string Before { get; set; }

        /// <summary>
        /// Pagination cursor to receive records after a provided identifier.
        /// </summary>
        [JsonProperty("after")]
        public string After { get; set; }

        /// <summary>
        /// Maximum number of records to return.
        /// </summary>
        [JsonProperty("limit")]
        public long? Limit { get; set; }

        /// <summary>
        /// The order in which to paginate records.
        /// </summary>
        [JsonProperty("order")]
        public PaginationOrder? Order { get; set; }
    }
}
