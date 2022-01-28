namespace WorkOS
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a paginated list of WorkOS records.
    /// </summary>
    /// <typeparam name="T">Type of WorkOS entity.</typeparam>
    [JsonObject]
    public class WorkOSList<T>
    {
        /// <summary>
        /// List of WorkOS records.
        /// </summary>
        [JsonProperty("data")]
        public List<T> Data { get; set; }

        /// <summary>
        /// Pagination cursor options.
        /// </summary>
        [JsonProperty("List_Metadata")]
        public ListMetadata ListMetadata { get; set; }
    }
}
