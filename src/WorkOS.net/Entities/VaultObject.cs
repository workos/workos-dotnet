// @oagen-ignore-file
namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>A Vault KV object.</summary>
    public class VaultObject
    {
        [JsonProperty("id")]
        public string Id { get; set; } = default!;

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("value")]
        public string? Value { get; set; }

        [JsonProperty("environment_id")]
        public string? EnvironmentId { get; set; }

        [JsonProperty("key_id")]
        public string? KeyId { get; set; }

        [JsonProperty("version_id")]
        public string? VersionId { get; set; }

        [JsonProperty("updated_at")]
        public string? UpdatedAt { get; set; }

        [JsonProperty("updated_by")]
        public string? UpdatedBy { get; set; }
    }
}
