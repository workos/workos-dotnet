// @oagen-ignore-file
namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>Options for updating a Vault object.</summary>
    public class UpdateVaultObjectOptions : BaseOptions
    {
        [JsonProperty("value")]
        public string Value { get; set; } = default!;

        [JsonProperty("version_check")]
        public string? VersionCheck { get; set; }
    }
}
