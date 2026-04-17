// @oagen-ignore-file
namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>A plaintext data key.</summary>
    public class DataKey
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; } = default!;
    }
}
