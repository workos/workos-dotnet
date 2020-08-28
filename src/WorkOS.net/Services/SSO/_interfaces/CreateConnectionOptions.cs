namespace WorkOS
{
    using Newtonsoft.Json;

    public class CreateConnectionOptions : BaseOptions
    {
        [JsonProperty("source")]
        public string Source { get; set; }
    }
}
