namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters used to activate a Draft Connection.
    /// </summary>
    public class CreateConnectionOptions : BaseOptions
    {
        /// <summary>
        /// The Draft Connection identifier.
        /// </summary>
        [JsonProperty("source")]
        public string Source { get; set; }
    }
}
