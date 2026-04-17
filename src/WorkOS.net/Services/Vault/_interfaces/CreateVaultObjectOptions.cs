// @oagen-ignore-file
namespace WorkOS
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>Options for creating a Vault object.</summary>
    public class CreateVaultObjectOptions : BaseOptions
    {
        [JsonProperty("name")]
        public string Name { get; set; } = default!;

        [JsonProperty("value")]
        public string Value { get; set; } = default!;

        [JsonProperty("key_context")]
        public Dictionary<string, string>? KeyContext { get; set; }
    }
}
