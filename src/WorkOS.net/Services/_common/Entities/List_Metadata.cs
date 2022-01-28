namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// Contains pagination options for WorkOS records.
    /// </summary>
    public class List_Metadata
    {
        /// <summary>
        /// Cursor to receive records before a provided identifier.
        /// </summary>
        [JsonProperty("before")]
        public string Before { get; set; }

        /// <summary>
        /// Cursor to receive records after a provided identifier.
        /// </summary>
        [JsonProperty("after")]
        public string After { get; set; }
    }
}
