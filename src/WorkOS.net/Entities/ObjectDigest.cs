// @oagen-ignore-file
namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>A Vault object digest (summary).</summary>
    public class ObjectDigest
    {
        [JsonProperty("id")]
        public string Id { get; set; } = default!;

        [JsonProperty("name")]
        public string Name { get; set; } = default!;

        [JsonProperty("updated_at")]
        public string? UpdatedAt { get; set; }
    }
}
