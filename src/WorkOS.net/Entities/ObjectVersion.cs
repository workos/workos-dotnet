// @oagen-ignore-file
namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>A Vault object version.</summary>
    public class ObjectVersion
    {
        [JsonProperty("version_id")]
        public string VersionId { get; set; } = default!;

        [JsonProperty("updated_at")]
        public string? UpdatedAt { get; set; }

        [JsonProperty("updated_by")]
        public string? UpdatedBy { get; set; }
    }
}
