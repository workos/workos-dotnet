namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// Arguments when fetching a List of WorkOS records.
    /// </summary>
    public class AutoPaginationOptions : BaseOptions
    {
        [JsonProperty("limit")]
        public long? Limit { get; set; }

        /// <summary>
        /// The order in which to paginate records.
        /// </summary>
        [JsonProperty("order")]
        public PaginationOrder? Order { get; set; }
    }
}
