namespace WorkOS
{
    using Newtonsoft.Json;

    public class EnrollFactorOptions : BaseOptions
    {
        /// <summary>
        /// Describes which type to use.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
